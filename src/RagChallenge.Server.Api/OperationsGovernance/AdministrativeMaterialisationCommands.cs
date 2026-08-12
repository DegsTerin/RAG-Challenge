// Purpose: Composes the two materialisation administration commands from explicit trusted dependencies while preserving plan validation, rights gates, immutable content, deterministic indexing, and fail-closed outcomes.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal interface IAdministrativeMaterialisationCommand
{
    string CommandName { get; }

    AdministrativeCommandIdentifiers DescribeIntent(
        CorpusId corpusId,
        JsonElement? input);

    Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default);
}

internal sealed class OfficialSynchronisationAdministrativeCommand(
    IControlPlaneStore controlPlaneStore,
    IOfficialSourceAuthorityResolver authorityResolver,
    OfficialSourceSynchronisationService synchronisationService)
    : IAdministrativeMaterialisationCommand
{
    public string CommandName => "synchronise-official";

    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));
    private readonly IOfficialSourceAuthorityResolver authorityResolver = authorityResolver ??
        throw new ArgumentNullException(nameof(authorityResolver));
    private readonly OfficialSourceSynchronisationService synchronisationService =
        synchronisationService ?? throw new ArgumentNullException(nameof(synchronisationService));

    public AdministrativeCommandIdentifiers DescribeIntent(
        CorpusId corpusId,
        JsonElement? input)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        var payload = ReadPayload(input, corpusId);
        var sources = new List<string>
        {
            $"catalogue-revision:{payload.ExpectedCatalogueRevision}",
            $"registration:{payload.RegistrationId}:{payload.RegistrationRevision}",
        };

        if (payload.ExpectedCurrentSnapshotId is not null)
        {
            sources.Add($"snapshot:{payload.ExpectedCurrentSnapshotId}");
        }

        return new AdministrativeCommandIdentifiers(
            sources,
            [$"observation:{payload.ObservationId}"]);
    }

    public async Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var payload = ReadPayload(command.Input, command.CorpusId);
        var registrationId = new OfficialSourceRegistrationId(payload.RegistrationId);
        var authority = await authorityResolver.ResolveAsync(
            command.CorpusId,
            registrationId,
            cancellationToken).ConfigureAwait(false);

        if (authority is null)
        {
            return Rejected("CH_ADMIN_NOT_FOUND");
        }

        if (!AuthorityMatches(payload, authority))
        {
            return Rejected(
                "CH_ADMIN_VALIDATION_FAILED",
                authority.ObservationJournalRevision);
        }

        var catalogue = await controlPlaneStore.ReadCurrentCatalogueAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var context = payload.Document.ToDomain(command.CorpusId);
        var rights = payload.Rights.ToDomain();

        if (!CatalogueMatches(
                catalogue,
                payload.ExpectedCatalogueRevision,
                authority,
                context) ||
            rights.DocumentId != context.DocumentId ||
            rights.DocumentVersion != context.DocumentVersion ||
            !DocumentRightsEligibilityPolicy.Evaluate(
                rights,
                DocumentRightsEligibilityGate.TextualEvidence).IsEligible)
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }

        var parserPolicy = payload.ParserPolicy.ToDomain();
        var chunkingPolicy = payload.ChunkingPolicy.ToDomain();

        if (payload.MaxAgeSeconds is <= 0 or > 31_536_000 ||
            payload.CurrentEtag?.Length > 512 ||
            payload.CurrentEtag?.Any(char.IsControl) == true ||
            payload.CurrentLastModified is { Offset: not { Ticks: 0 } })
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }

        var observationAudit = new AdministrativeAuditContext(
            AdministrativeChildOperationIds.CreateOfficialObservation(command.OperationId),
            command.AuditContext.ActorIdentifier,
            command.AuditContext.Command,
            command.AuditContext.Reason,
            command.AuditContext.RequestedAt);

        try
        {
            var result = await synchronisationService.SynchroniseAsync(
                new OfficialSynchronisationRequest(
                    command.CorpusId,
                    authority.Registration,
                    context.DocumentFormat,
                    context,
                    parserPolicy,
                    chunkingPolicy,
                    parserPolicy.MaximumByteLength,
                    command.AuditContext,
                    observationAudit,
                    authority.ObservationJournalRevision,
                    new OfficialObservationId(payload.ObservationId),
                    TimeSpan.FromSeconds(payload.MaxAgeSeconds),
                    authority.CurrentSnapshot,
                    payload.CurrentEtag,
                    payload.CurrentLastModified,
                    authority.ActivationRevision),
                cancellationToken).ConfigureAwait(false);
            return new AdministrativeExecutionResult(
                AdministrativeExecutionOutcome.Applied,
                "CH_ADMIN_APPLIED",
                result.Observation.JournalRevision.Value);
        }
        catch (DocumentParseException)
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }
    }

    private static bool AuthorityMatches(
        OfficialSyncPayload payload,
        OfficialSourceAuthority authority) =>
        authority.Registration.Id.Value == payload.RegistrationId &&
        authority.Registration.Revision.Value == payload.RegistrationRevision &&
        authority.ObservationJournalRevision == payload.ExpectedJournalRevision &&
        authority.ActivationRevision == payload.ExpectedActivationRevision &&
        string.Equals(
            authority.CurrentSnapshot?.Id.Value,
            payload.ExpectedCurrentSnapshotId,
            StringComparison.Ordinal) &&
        authority.Registration.Status is CatalogueItemStatus.Candidate or
            CatalogueItemStatus.Active;

    private static OfficialSyncPayload ReadPayload(
        JsonElement? input,
        CorpusId corpusId)
    {
        var payload = AdministrativeMaterialisationJson.Deserialize<OfficialSyncPayload>(
            input);

        if (payload.Document is null ||
            payload.ParserPolicy is null ||
            payload.ChunkingPolicy is null ||
            payload.Rights is null ||
            payload.ExpectedJournalRevision < 0 ||
            payload.ExpectedActivationRevision < 0)
        {
            throw new InvalidDataException(
                "The official synchronisation plan is incomplete or invalid.");
        }

        _ = new OfficialSourceRegistrationId(payload.RegistrationId);
        _ = new SourceRegistrationRevision(payload.RegistrationRevision);
        _ = new OfficialObservationId(payload.ObservationId);

        if (payload.ExpectedCurrentSnapshotId is not null)
        {
            _ = new OfficialSnapshotId(payload.ExpectedCurrentSnapshotId);
        }

        _ = payload.Document.ToDomain(corpusId);
        _ = payload.ParserPolicy.ToDomain();
        _ = payload.ChunkingPolicy.ToDomain();
        _ = payload.Rights.ToDomain();
        return payload;
    }

    private static bool CatalogueMatches(
        CatalogueSnapshot? catalogue,
        long expectedCatalogueRevision,
        OfficialSourceAuthority authority,
        DocumentChunkingContext context)
    {
        if (catalogue is null ||
            catalogue.Revision.Value != expectedCatalogueRevision)
        {
            return false;
        }

        var product = catalogue.DatabaseProducts.SingleOrDefault(candidate =>
            candidate.Id == context.DatabaseProductId);
        var document = catalogue.DocumentVersions.SingleOrDefault(candidate =>
            candidate.Id == context.DocumentId &&
            candidate.Version == context.DocumentVersion);

        if (product is null || document is null ||
            product.Revision != context.DatabaseProductRevision ||
            product.Status is not (CatalogueItemStatus.Candidate or
                CatalogueItemStatus.Active) ||
            document.DatabaseProductId != context.DatabaseProductId ||
            document.DatabaseProductRevision != context.DatabaseProductRevision ||
            document.Format != context.DocumentFormat ||
            document.ContentLanguage != context.ContentLanguage ||
            document.Status is not (CatalogueItemStatus.Candidate or
                CatalogueItemStatus.Active))
        {
            return false;
        }

        var snapshot = authority.CurrentSnapshot;
        return snapshot is null
            ? document.Status == CatalogueItemStatus.Candidate &&
                document.SourceTrustClass == SourceTrustClass.LocalAuthorised &&
                document.OfficialSourceRegistrationId is null &&
                document.OfficialSnapshotId is null
            : document.SourceAdapterId == context.SourceAdapterId &&
                document.SourceTrustClass == SourceTrustClass.OfficialExternal &&
                document.OfficialSourceRegistrationId == authority.Registration.Id &&
                document.OfficialSnapshotId == snapshot.Id &&
                document.ContentObjectId == snapshot.ContentObjectId &&
                document.ByteLength == snapshot.ByteLength &&
                string.Equals(document.MediaType, snapshot.MediaType, StringComparison.Ordinal);
    }

    private static AdministrativeExecutionResult Rejected(
        string resultCode,
        long? resultRevision = null) =>
        new(AdministrativeExecutionOutcome.Rejected, resultCode, resultRevision);

    private sealed class OfficialSyncPayload
    {
        public long ExpectedCatalogueRevision { get; init; }

        public required string RegistrationId { get; init; }

        public long RegistrationRevision { get; init; }

        public string? ExpectedCurrentSnapshotId { get; init; }

        public long ExpectedJournalRevision { get; init; }

        public long ExpectedActivationRevision { get; init; }

        public required string ObservationId { get; init; }

        public long MaxAgeSeconds { get; init; }

        public string? CurrentEtag { get; init; }

        public DateTimeOffset? CurrentLastModified { get; init; }

        public required OfficialDocumentPayload Document { get; init; }

        public required ParserPolicyPayload ParserPolicy { get; init; }

        public required ChunkingPolicyPayload ChunkingPolicy { get; init; }

        public required DocumentRightsPayload Rights { get; init; }
    }

    private sealed class OfficialDocumentPayload
    {
        public required string DatabaseProductId { get; init; }

        public long DatabaseProductRevision { get; init; }

        public required string DocumentId { get; init; }

        public long DocumentVersion { get; init; }

        public required string DocumentFormat { get; init; }

        public required string ContentLanguage { get; init; }

        public required string SourceAdapterId { get; init; }

        internal DocumentChunkingContext ToDomain(CorpusId corpusId) =>
            new(
                corpusId,
                new DatabaseProductId(DatabaseProductId),
                new DatabaseProductRevision(DatabaseProductRevision),
                new DocumentId(DocumentId),
                new DocumentVersionNumber(DocumentVersion),
                AdministrativeMaterialisationJson.ParseEnum<DocumentFormat>(DocumentFormat),
                AdministrativeMaterialisationJson.ParseCanonicalLanguage(ContentLanguage),
                new SourceAdapterId(SourceAdapterId),
                SourceTrustClass.OfficialExternal);
    }
}

internal sealed class BuildIndexAdministrativeCommand(
    IControlPlaneStore controlPlaneStore,
    IDocumentContentStore contentStore,
    DocumentIngestionService ingestionService,
    CorpusIndexingService indexingService,
    IndexCompatibilityProfile compatibilityProfile)
    : IAdministrativeMaterialisationCommand
{
    public string CommandName => "build-index";

    private readonly IControlPlaneStore controlPlaneStore = controlPlaneStore ??
        throw new ArgumentNullException(nameof(controlPlaneStore));
    private readonly IDocumentContentStore contentStore = contentStore ??
        throw new ArgumentNullException(nameof(contentStore));
    private readonly DocumentIngestionService ingestionService = ingestionService ??
        throw new ArgumentNullException(nameof(ingestionService));
    private readonly CorpusIndexingService indexingService = indexingService ??
        throw new ArgumentNullException(nameof(indexingService));
    private readonly IndexCompatibilityProfile compatibilityProfile = compatibilityProfile ??
        throw new ArgumentNullException(nameof(compatibilityProfile));

    public AdministrativeCommandIdentifiers DescribeIntent(
        CorpusId corpusId,
        JsonElement? input)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        var payload = ReadPayload(input);
        var candidateBuildId = new CandidateBuildId(payload.CandidateBuildId);
        return new AdministrativeCommandIdentifiers(
            [$"catalogue-revision:{payload.CatalogueRevision}"],
            [$"candidate-build:{candidateBuildId.Value}"]);
    }

    public async Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var payload = ReadPayload(command.Input);

        if (!string.Equals(
                payload.ExpectedIndexCompatibilityKey,
                compatibilityProfile.Key.Value,
                StringComparison.Ordinal) ||
            payload.MaximumEmbeddingBatchUtf8Bytes is <= 0 or > 4 * 1024 * 1024)
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }

        var catalogue = await controlPlaneStore.ReadCurrentCatalogueAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);

        if (!CatalogueMatches(catalogue, payload))
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }

        var documents = new List<IndexDocumentInput>(payload.Documents.Length);

        try
        {
            foreach (var document in payload.Documents)
            {
                var binding = document.Binding.ToDomain();
                var rights = document.Rights.ToDomain();

                if (rights.DocumentId != binding.DocumentId ||
                    rights.DocumentVersion != binding.DocumentVersion ||
                    !DocumentRightsEligibilityPolicy.Evaluate(
                        rights,
                        DocumentRightsEligibilityGate.TextualEvidence).IsEligible ||
                    document.ParserPolicy.MaximumByteLength < document.ByteLength)
                {
                    return Rejected("CH_ADMIN_VALIDATION_FAILED");
                }

                var contentObjectId = new ContentObjectId(document.SourceContentObjectId);
                await using var verified = await contentStore.OpenVerifiedAsync(
                    contentObjectId,
                    new ExpectedHashAndLength(contentObjectId, document.ByteLength),
                    cancellationToken).ConfigureAwait(false);
                var context = new DocumentChunkingContext(
                    command.CorpusId,
                    binding.DatabaseProductId,
                    binding.DatabaseProductRevision,
                    binding.DocumentId,
                    binding.DocumentVersion,
                    binding.DocumentFormat,
                    AdministrativeMaterialisationJson.ParseCanonicalLanguage(
                        document.ContentLanguage),
                    binding.SourceAdapterId,
                    binding.SourceTrustClass);
                var mediaType = new ContentMediaType(document.MediaType);
                var ingestion = await ingestionService.IngestAsync(
                    new DocumentIngestionRequest(
                        verified.Content,
                        document.ParserPolicy.MaximumByteLength,
                        mediaType,
                        document.ParserPolicy.ToDomain(),
                        compatibilityProfile.ChunkingPolicy,
                        context,
                        contentObjectId),
                    cancellationToken).ConfigureAwait(false);

                if (ingestion.Content.ContentObjectId != contentObjectId ||
                    ingestion.Content.ByteLength != document.ByteLength ||
                    ingestion.Content.MediaType != mediaType)
                {
                    return Rejected("CH_ADMIN_VALIDATION_FAILED");
                }

                documents.Add(new IndexDocumentInput(
                    binding,
                    context.ContentLanguage,
                    ingestion.Chunks,
                    ingestion.ParsedArtifact.ParserDescriptor,
                    compatibilityProfile.ChunkingPolicy));
            }

            var specification = new IndexGenerationSpecification(
                manifestSchemaVersion: 1,
                command.CorpusId,
                new CorpusRevision(payload.CorpusRevision),
                new CatalogueRevision(payload.CatalogueRevision),
                new ActiveDocumentSetDigest(payload.ActiveDocumentSetDigest),
                new SourceBindingSetDigest(payload.SourceBindingSetDigest),
                compatibilityProfile.Key);
            var result = await indexingService.BuildAsync(
                new CorpusIndexingRequest(
                    new CandidateBuildId(payload.CandidateBuildId),
                    specification,
                    documents,
                    compatibilityProfile.EmbeddingDescriptor,
                    compatibilityProfile,
                    command.AuditContext,
                    command.AuditContext.RequestedAt,
                    payload.MaximumEmbeddingBatchUtf8Bytes),
                cancellationToken).ConfigureAwait(false);
            return result.CommitResult.Outcome switch
            {
                StoreMutationOutcome.Applied => new AdministrativeExecutionResult(
                    AdministrativeExecutionOutcome.Applied,
                    "CH_ADMIN_APPLIED",
                    result.CommitResult.CurrentRevision),
                StoreMutationOutcome.AlreadyApplied => new AdministrativeExecutionResult(
                    AdministrativeExecutionOutcome.AlreadyApplied,
                    "CH_ADMIN_APPLIED",
                    result.CommitResult.CurrentRevision),
                _ => Rejected(
                    AdministrativeMaterialisationJson.MapStoreFailure(
                        result.CommitResult.Outcome),
                    result.CommitResult.CurrentRevision),
            };
        }
        catch (DocumentParseException)
        {
            return Rejected("CH_ADMIN_VALIDATION_FAILED");
        }
    }

    private static bool CatalogueMatches(
        CatalogueSnapshot? catalogue,
        BuildIndexPayload payload)
    {
        if (catalogue is null || catalogue.Revision.Value != payload.CatalogueRevision)
        {
            return false;
        }

        var activeDocuments = catalogue.DocumentVersions
            .Where(document => document.Status == CatalogueItemStatus.Active)
            .ToArray();

        if (activeDocuments.Length == 0 ||
            activeDocuments.Length != payload.Documents.Length)
        {
            return false;
        }

        var products = catalogue.DatabaseProducts.ToDictionary(product => product.Id);
        var requested = new HashSet<(DocumentId, DocumentVersionNumber)>();

        foreach (var documentPayload in payload.Documents)
        {
            var binding = documentPayload.Binding.ToDomain();

            if (!requested.Add((binding.DocumentId, binding.DocumentVersion)))
            {
                return false;
            }

            var document = activeDocuments.SingleOrDefault(candidate =>
                candidate.Id == binding.DocumentId &&
                candidate.Version == binding.DocumentVersion);

            if (document is null ||
                !products.TryGetValue(binding.DatabaseProductId, out var product) ||
                product.Status != CatalogueItemStatus.Active ||
                product.Revision != binding.DatabaseProductRevision ||
                document.DatabaseProductId != binding.DatabaseProductId ||
                document.DatabaseProductRevision != binding.DatabaseProductRevision ||
                document.Format != binding.DocumentFormat ||
                document.ContentLanguage !=
                    AdministrativeMaterialisationJson.ParseCanonicalLanguage(
                        documentPayload.ContentLanguage) ||
                document.ContentObjectId.Value != documentPayload.SourceContentObjectId ||
                document.ByteLength != documentPayload.ByteLength ||
                !string.Equals(
                    document.MediaType,
                    new ContentMediaType(documentPayload.MediaType).Value,
                    StringComparison.Ordinal) ||
                document.SourceAdapterId != binding.SourceAdapterId ||
                document.SourceTrustClass != binding.SourceTrustClass ||
                document.OfficialSourceRegistrationId !=
                    binding.OfficialSourceRegistrationId ||
                document.OfficialSnapshotId != binding.OfficialSnapshotId)
            {
                return false;
            }
        }

        return true;
    }

    private static BuildIndexPayload ReadPayload(JsonElement? input)
    {
        var payload = AdministrativeMaterialisationJson.Deserialize<BuildIndexPayload>(
            input);

        if (payload.Documents is null ||
            payload.Documents.Length == 0 ||
            payload.Documents.Any(document =>
                document is null ||
                document.Binding is null ||
                document.ParserPolicy is null ||
                document.Rights is null))
        {
            throw new InvalidDataException(
                "The index-build plan is incomplete or empty.");
        }

        _ = new CandidateBuildId(payload.CandidateBuildId);
        _ = new CorpusRevision(payload.CorpusRevision);
        _ = new CatalogueRevision(payload.CatalogueRevision);
        _ = new ActiveDocumentSetDigest(payload.ActiveDocumentSetDigest);
        _ = new SourceBindingSetDigest(payload.SourceBindingSetDigest);
        _ = new IndexCompatibilityKey(payload.ExpectedIndexCompatibilityKey);

        foreach (var document in payload.Documents)
        {
            _ = document.Binding.ToDomain();
            _ = AdministrativeMaterialisationJson.ParseCanonicalLanguage(
                document.ContentLanguage);
            var contentObjectId = new ContentObjectId(document.SourceContentObjectId);
            _ = new ExpectedHashAndLength(contentObjectId, document.ByteLength);
            _ = new ContentMediaType(document.MediaType);
            _ = document.ParserPolicy.ToDomain();
            _ = document.Rights.ToDomain();
        }

        return payload;
    }

    private static AdministrativeExecutionResult Rejected(
        string resultCode,
        long? resultRevision = null) =>
        new(AdministrativeExecutionOutcome.Rejected, resultCode, resultRevision);

    private sealed class BuildIndexPayload
    {
        public required string CandidateBuildId { get; init; }

        public long CorpusRevision { get; init; }

        public long CatalogueRevision { get; init; }

        public required string ActiveDocumentSetDigest { get; init; }

        public required string SourceBindingSetDigest { get; init; }

        public required string ExpectedIndexCompatibilityKey { get; init; }

        public int MaximumEmbeddingBatchUtf8Bytes { get; init; }

        public required BuildDocumentPayload[] Documents { get; init; }
    }

    private sealed class BuildDocumentPayload
    {
        public required DocumentBindingPayload Binding { get; init; }

        public required string ContentLanguage { get; init; }

        public required string SourceContentObjectId { get; init; }

        public long ByteLength { get; init; }

        public required string MediaType { get; init; }

        public required ParserPolicyPayload ParserPolicy { get; init; }

        public required DocumentRightsPayload Rights { get; init; }
    }
}

internal static class AdministrativeChildOperationIds
{
    private const string OfficialObservationDomain =
        "rag-admin-child-operation-v1\nofficial-observation\n";

    internal static OperationId CreateOfficialObservation(OperationId ownerOperationId)
    {
        ArgumentNullException.ThrowIfNull(ownerOperationId);
        var bytes = Encoding.UTF8.GetBytes(
            OfficialObservationDomain + ownerOperationId.Value);
        var digest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return new OperationId("admin-child-" + digest);
    }
}

internal static class AdministrativeMaterialisationJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    internal static T Deserialize<T>(JsonElement? input)
        where T : class =>
        input?.Deserialize<T>(Options) ??
            throw new InvalidDataException("The administrative input plan was empty.");

    internal static T ParseEnum<T>(string value)
        where T : struct, Enum =>
        Enum.Parse<T>(value, ignoreCase: false);

    internal static DocumentContentLanguage ParseCanonicalLanguage(string value)
    {
        var language = new DocumentContentLanguage(value);

        if (!string.Equals(language.ToCanonicalTag(), value, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "A document language in an administrative plan must already be canonical.");
        }

        return language;
    }

    internal static string MapStoreFailure(StoreMutationOutcome outcome) =>
        outcome switch
        {
            StoreMutationOutcome.RevisionConflict => "CH_ADMIN_REVISION_CONFLICT",
            StoreMutationOutcome.ValidationFailed => "CH_ADMIN_VALIDATION_FAILED",
            StoreMutationOutcome.NotFound => "CH_ADMIN_NOT_FOUND",
            StoreMutationOutcome.RetentionConflict => "CH_ADMIN_LEASE_CONFLICT",
            _ => "CH_ADMIN_COMMAND_REJECTED",
        };
}

internal sealed class ParserPolicyPayload
{
    public long MaximumByteLength { get; init; }

    public int MaximumUnits { get; init; }

    public int MaximumTextCharacters { get; init; }

    public int MaximumFieldsPerRecord { get; init; }

    public int MaximumFieldCharacters { get; init; }

    internal ParserPolicy ToDomain() =>
        new(
            MaximumByteLength,
            MaximumUnits,
            MaximumTextCharacters,
            MaximumFieldsPerRecord,
            MaximumFieldCharacters);
}

internal sealed class ChunkingPolicyPayload
{
    public int TargetScalarCount { get; init; }

    public int OverlapScalarCount { get; init; }

    public int HardMaximumScalarCount { get; init; }

    internal ChunkingPolicy ToDomain() =>
        new(TargetScalarCount, OverlapScalarCount, HardMaximumScalarCount);
}

internal sealed class DocumentBindingPayload
{
    public required string DatabaseProductId { get; init; }

    public long DatabaseProductRevision { get; init; }

    public required string DocumentId { get; init; }

    public long DocumentVersion { get; init; }

    public required string DocumentFormat { get; init; }

    public required string SourceAdapterId { get; init; }

    public required string SourceTrustClass { get; init; }

    public string? OfficialSourceRegistrationId { get; init; }

    public string? OfficialSnapshotId { get; init; }

    public string? SourceObservationId { get; init; }

    internal DocumentBinding ToDomain() =>
        new(
            new DatabaseProductId(DatabaseProductId),
            new DatabaseProductRevision(DatabaseProductRevision),
            new DocumentId(DocumentId),
            new DocumentVersionNumber(DocumentVersion),
            AdministrativeMaterialisationJson.ParseEnum<DocumentFormat>(DocumentFormat),
            new SourceAdapterId(SourceAdapterId),
            AdministrativeMaterialisationJson.ParseEnum<SourceTrustClass>(SourceTrustClass),
            OfficialSourceRegistrationId is null
                ? null
                : new OfficialSourceRegistrationId(OfficialSourceRegistrationId),
            OfficialSnapshotId is null
                ? null
                : new OfficialSnapshotId(OfficialSnapshotId),
            SourceObservationId is null
                ? null
                : new OfficialObservationId(SourceObservationId));
}

internal sealed class DocumentRightsPayload
{
    public int RightsSchemaVersion { get; init; }

    public required DocumentRightDecisionPayload[] RightsDecisions { get; init; }

    public required string DocumentId { get; init; }

    public long DocumentVersion { get; init; }

    internal DocumentRightsEligibilityRecordV1 ToDomain()
    {
        if (RightsSchemaVersion != DocumentRightsEligibilityRecordV1.CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                "The rights record schema is not supported by this administration runtime.");
        }

        return new DocumentRightsEligibilityRecordV1(
            new DocumentId(DocumentId),
            new DocumentVersionNumber(DocumentVersion),
            RightsDecisions.Select(decision => decision.ToDomain()));
    }
}

internal sealed class DocumentRightDecisionPayload
{
    public required string Right { get; init; }

    public required string State { get; init; }

    public required string EvidenceReference { get; init; }

    internal DocumentRightDecision ToDomain() =>
        new(
            AdministrativeMaterialisationJson.ParseEnum<DocumentRight>(Right),
            AdministrativeMaterialisationJson.ParseEnum<DocumentRightDecisionState>(State),
            new DocumentRightsEvidenceReference(EvidenceReference));
}
