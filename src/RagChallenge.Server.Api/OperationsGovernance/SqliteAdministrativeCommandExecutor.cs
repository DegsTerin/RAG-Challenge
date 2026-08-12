// Purpose: Adapts strict one-shot JSON plans to existing administrative use cases while optional materialisation capabilities remain explicitly composed and fail closed.
using System.Text.Json;
using System.Text.Json.Serialization;

using RagChallenge.Application.Administration;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed class SqliteAdministrativeCommandExecutor
    : IOneShotAdministrativeCommandExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly IControlPlaneStore store;
    private readonly IAdministrativeMaterialisationCommand? synchroniseOfficial;
    private readonly IAdministrativeMaterialisationCommand? buildIndex;

    internal SqliteAdministrativeCommandExecutor(
        IControlPlaneStore store,
        IAdministrativeMaterialisationCommand? synchroniseOfficial = null,
        IAdministrativeMaterialisationCommand? buildIndex = null)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.synchroniseOfficial = ValidateMaterialisationCommand(
            synchroniseOfficial,
            "synchronise-official");
        this.buildIndex = ValidateMaterialisationCommand(buildIndex, "build-index");
    }

    public AdministrativeCommandIdentifiers DescribeIntent(
        string command,
        CorpusId corpusId,
        JsonElement? input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(corpusId);

        return command switch
        {
            "add-database" or
            "version-database" or
            "activate-database" or
            "deactivate-database" or
            "remove-database" or
            "add-document" or
            "version-document" or
            "activate-document" or
            "deactivate-document" or
            "remove-document" => DescribeCatalogueIntent(input),
            "register-official-source" => DescribeOfficialSourceIntent(input),
            "activate-generation" or "rollback-generation" =>
                DescribeGenerationIntent(input),
            "synchronise-official" => synchroniseOfficial?.DescribeIntent(
                corpusId,
                input) ?? DescribeUnavailableMaterialisationIntent(corpusId),
            "build-index" => buildIndex?.DescribeIntent(
                corpusId,
                input) ?? DescribeUnavailableMaterialisationIntent(corpusId),
            "status" => new(
                [$"corpus:{corpusId.Value}"],
                []),
            _ => throw new InvalidDataException(
                "The administrative command is outside the accepted surface."),
        };
    }

    public Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default) =>
        command.Command switch
        {
            "add-database" or
            "version-database" or
            "activate-database" or
            "deactivate-database" or
            "remove-database" or
            "add-document" or
            "version-document" or
            "activate-document" or
            "deactivate-document" or
            "remove-document" => ExecuteCatalogueAsync(command, cancellationToken),
            "register-official-source" =>
                RegisterOfficialSourceAsync(command, cancellationToken),
            "activate-generation" => ActivateGenerationAsync(command, cancellationToken),
            "rollback-generation" => RollbackGenerationAsync(command, cancellationToken),
            "status" => ReadStatusAsync(command, cancellationToken),
            "synchronise-official" => ExecuteMaterialisationAsync(
                synchroniseOfficial,
                command,
                cancellationToken),
            "build-index" => ExecuteMaterialisationAsync(
                buildIndex,
                command,
                cancellationToken),
            _ => Task.FromResult(new AdministrativeExecutionResult(
                AdministrativeExecutionOutcome.Rejected,
                "CH_ADMIN_COMMAND_REJECTED")),
        };

    private static AdministrativeCommandIdentifiers DescribeUnavailableMaterialisationIntent(
        CorpusId corpusId) =>
        new([$"corpus:{corpusId.Value}"], []);

    private static IAdministrativeMaterialisationCommand? ValidateMaterialisationCommand(
        IAdministrativeMaterialisationCommand? materialisationCommand,
        string expectedCommand)
    {
        if (materialisationCommand is not null &&
            !string.Equals(
                materialisationCommand.CommandName,
                expectedCommand,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An administrative materialisation command was composed on the wrong route.",
                nameof(materialisationCommand));
        }

        return materialisationCommand;
    }

    private static Task<AdministrativeExecutionResult> ExecuteMaterialisationAsync(
        IAdministrativeMaterialisationCommand? materialisationCommand,
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken) =>
        materialisationCommand?.ExecuteAsync(command, cancellationToken) ??
        Task.FromResult(new AdministrativeExecutionResult(
            AdministrativeExecutionOutcome.Unavailable,
            "CH_ADMIN_CAPABILITY_NOT_COMPOSED"));

    private async Task<AdministrativeExecutionResult> ExecuteCatalogueAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<CatalogueCommandPayload>(command);
        var proposed = payload.ToDomain(command.CorpusId);
        var current = await store.ReadCurrentCatalogueAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);
        CatalogueCommandPolicy.Validate(
            command.Command,
            payload.TargetId,
            payload.TargetVersion,
            current,
            proposed,
            payload.ExpectedCurrentRevision);
        var result = await new CatalogueAdministrationService(store).ApplyAsync(
            new CatalogueAdministrationRequest(
                proposed,
                payload.ExpectedCurrentRevision,
                command.AuditContext,
                command.InputSha256,
                SuccessCompletion(command, proposed.Revision.Value)),
            cancellationToken).ConfigureAwait(false);
        return Map(result, completionRecordedOnSuccess: true);
    }

    private async Task<AdministrativeExecutionResult> RegisterOfficialSourceAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<OfficialSourceRegistrationPayload>(command);
        var registration = payload.ToDomain();
        var result = await store.RegisterOfficialSourceAsync(
            new OfficialSourceRegistrationCommitRequest(
                command.OperationId,
                command.CorpusId,
                registration,
                command.AuditContext.RequestedAt,
                command.AuditContext.CreateDigest(
                    registration.Id.Value,
                    registration.Revision.ToCanonicalString(),
                    registration.DatabaseProductId.Value,
                    registration.DocumentId.Value,
                    command.InputSha256 ?? "none"),
                SuccessCompletion(command, registration.Revision.Value)),
            cancellationToken).ConfigureAwait(false);
        return Map(result, completionRecordedOnSuccess: true);
    }

    private async Task<AdministrativeExecutionResult> ActivateGenerationAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<GenerationActivationPayload>(command);
        var manifest = payload.Manifest.ToDomain(command.CorpusId);
        var evidenceBindings = payload.EvidenceBindings
            .Select(binding => binding.ToDomain())
            .ToArray();
        var result = await new GenerationActivationService(store).ActivateAsync(
            new GenerationActivationRequest(
                manifest,
                evidenceBindings,
                payload.ExpectedCurrentRevision,
                TimeSpan.FromDays(payload.PreviousGenerationRetentionDays),
                command.AuditContext,
                SuccessCompletion(command, payload.ExpectedCurrentRevision + 1)),
            cancellationToken).ConfigureAwait(false);
        return Map(result, completionRecordedOnSuccess: true);
    }

    private async Task<AdministrativeExecutionResult> RollbackGenerationAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<GenerationActivationPayload>(command);
        var manifest = payload.Manifest.ToDomain(command.CorpusId);
        var evidenceBindings = payload.EvidenceBindings
            .Select(binding => binding.ToDomain())
            .ToArray();
        var current = await store.ReadActiveActivationAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);

        if (current is null || current.RecordRevision.Value != payload.ExpectedCurrentRevision)
        {
            return new AdministrativeExecutionResult(
                AdministrativeExecutionOutcome.Rejected,
                "CH_ADMIN_REVISION_CONFLICT",
                current?.RecordRevision.Value);
        }

        var proposed = ActivationRecordFactory.CreateRollback(
            current,
            manifest,
            evidenceBindings,
            command.AuditContext.RequestedAt);
        var result = await store.CompareExchangeActivationAsync(
            new ActivationCompareExchangeRequest(
                command.OperationId,
                ActivationMutationKind.Rollback,
                payload.ExpectedCurrentRevision,
                proposed,
                manifest.IndexCompatibilityKey,
                command.AuditContext.RequestedAt,
                TimeSpan.FromDays(payload.PreviousGenerationRetentionDays),
                command.AuditContext.CreateDigest(
                    manifest.IndexGenerationId.Value,
                    proposed.ActivationBindingSetDigest.Value,
                    command.InputSha256 ?? "none"),
                SuccessCompletion(command, proposed.RecordRevision.Value)),
            cancellationToken).ConfigureAwait(false);
        return Map(result, completionRecordedOnSuccess: true);
    }

    private async Task<AdministrativeExecutionResult> ReadStatusAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken)
    {
        var catalogue = await store.ReadCurrentCatalogueAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);
        var activation = await store.ReadActiveActivationAsync(
            command.CorpusId,
            cancellationToken).ConfigureAwait(false);
        return new AdministrativeExecutionResult(
            AdministrativeExecutionOutcome.Applied,
            catalogue is null ? "CH_ADMIN_STATUS_EMPTY" : "CH_ADMIN_STATUS_AVAILABLE",
            activation?.RecordRevision.Value ?? catalogue?.Revision.Value ?? 0);
    }

    private static T Deserialize<T>(OneShotAdministrativeCommand command)
        where T : class
    {
        if (command.Input is null)
        {
            throw new InvalidDataException("A mutating command requires an input plan.");
        }

        return Deserialize<T>(command.Input);
    }

    private static T Deserialize<T>(JsonElement? input)
        where T : class =>
        input?.Deserialize<T>(JsonOptions) ??
            throw new InvalidDataException("The administrative input plan was empty.");

    private static AdministrativeCommandIdentifiers DescribeCatalogueIntent(
        JsonElement? input)
    {
        var payload = Deserialize<CatalogueCommandPayload>(input);
        var target = payload.TargetVersion is null
            ? $"catalogue-item:{payload.TargetId}"
            : $"document-version:{payload.TargetId}:{payload.TargetVersion.Value}";
        return new(
            [$"catalogue-revision:{payload.ExpectedCurrentRevision}"],
            [target]);
    }

    private static AdministrativeCommandIdentifiers DescribeOfficialSourceIntent(
        JsonElement? input)
    {
        var payload = Deserialize<OfficialSourceRegistrationPayload>(input);
        return new(
            [
                $"database:{payload.DatabaseProductId}",
                $"document:{payload.DocumentId}",
            ],
            [$"registration:{payload.RegistrationId}"]);
    }

    private static AdministrativeCommandIdentifiers DescribeGenerationIntent(
        JsonElement? input)
    {
        var payload = Deserialize<GenerationActivationPayload>(input);
        return new(
            [$"activation-revision:{payload.ExpectedCurrentRevision}"],
            [$"generation:{payload.Manifest.IndexGenerationId}"]);
    }

    private static AdministrationJournalCompletion SuccessCompletion(
        OneShotAdministrativeCommand command,
        long resultRevision) =>
        new(
            command.OperationId,
            command.JournalIntentDigest,
            AdministrationJournalResultOutcome.Applied,
            "CH_ADMIN_APPLIED",
            (int)AdministrationExitCode.Success,
            resultRevision);

    private static AdministrativeExecutionResult Map(
        StoreMutationResult result,
        bool completionRecordedOnSuccess) =>
        result.Outcome switch
        {
            StoreMutationOutcome.Applied => new(
                AdministrativeExecutionOutcome.Applied,
                "CH_ADMIN_APPLIED",
                result.CurrentRevision,
                completionRecordedOnSuccess),
            StoreMutationOutcome.AlreadyApplied => new(
                AdministrativeExecutionOutcome.AlreadyApplied,
                "CH_ADMIN_APPLIED",
                result.CurrentRevision,
                completionRecordedOnSuccess),
            _ => new(
                AdministrativeExecutionOutcome.Rejected,
                MapStoreFailure(result.Outcome),
                result.CurrentRevision),
        };

    private static AdministrativeExecutionResult Map(
        ActivationMutationResult result,
        bool completionRecordedOnSuccess) =>
        result.Outcome switch
        {
            StoreMutationOutcome.Applied => new(
                AdministrativeExecutionOutcome.Applied,
                "CH_ADMIN_APPLIED",
                result.CurrentRecord?.RecordRevision.Value,
                completionRecordedOnSuccess),
            StoreMutationOutcome.AlreadyApplied => new(
                AdministrativeExecutionOutcome.AlreadyApplied,
                "CH_ADMIN_APPLIED",
                result.CurrentRecord?.RecordRevision.Value,
                completionRecordedOnSuccess),
            _ => new(
                AdministrativeExecutionOutcome.Rejected,
                MapStoreFailure(result.Outcome),
                result.CurrentRecord?.RecordRevision.Value),
        };

    private static string MapStoreFailure(StoreMutationOutcome outcome) =>
        outcome switch
        {
            StoreMutationOutcome.RevisionConflict => "CH_ADMIN_REVISION_CONFLICT",
            StoreMutationOutcome.ValidationFailed => "CH_ADMIN_VALIDATION_FAILED",
            StoreMutationOutcome.NotFound => "CH_ADMIN_NOT_FOUND",
            StoreMutationOutcome.RetentionConflict => "CH_ADMIN_LEASE_CONFLICT",
            _ => "CH_ADMIN_COMMAND_REJECTED",
        };

    private sealed class CatalogueCommandPayload
    {
        public required string TargetId { get; init; }

        public long? TargetVersion { get; init; }

        public long ExpectedCurrentRevision { get; init; }

        public long Revision { get; init; }

        public required DatabaseCategoryPayload[] Categories { get; init; }

        public required DatabaseProductPayload[] DatabaseProducts { get; init; }

        public required DocumentVersionPayload[] DocumentVersions { get; init; }

        internal CatalogueSnapshot ToDomain(CorpusId corpusId) =>
            new(
                corpusId,
                new CatalogueRevision(Revision),
                Categories.Select(category => category.ToDomain()),
                DatabaseProducts.Select(product => product.ToDomain()),
                DocumentVersions.Select(document => document.ToDomain()));
    }

    private sealed class DatabaseCategoryPayload
    {
        public required string Id { get; init; }

        public required string DisplayName { get; init; }

        internal DatabaseCategory ToDomain() =>
            new(new DatabaseCategoryId(Id), DisplayName);
    }

    private sealed class DatabaseProductPayload
    {
        public required string Id { get; init; }

        public long Revision { get; init; }

        public required string DisplayName { get; init; }

        public required string Status { get; init; }

        public required string[] CategoryIds { get; init; }

        internal DatabaseProduct ToDomain() =>
            new(
                new DatabaseProductId(Id),
                new DatabaseProductRevision(Revision),
                DisplayName,
                ParseStatus(Status),
                CategoryIds.Select(value => new DatabaseCategoryId(value)));
    }

    private sealed class DocumentVersionPayload
    {
        public required string Id { get; init; }

        public long Version { get; init; }

        public required string DatabaseProductId { get; init; }

        public long DatabaseProductRevision { get; init; }

        public required string Format { get; init; }

        public required string ContentLanguage { get; init; }

        public string? SourceDeclaredLanguage { get; init; }

        public required string Status { get; init; }

        public required string ContentObjectId { get; init; }

        public long ByteLength { get; init; }

        public required string MediaType { get; init; }

        public required string SourceAdapterId { get; init; }

        public required string SourceTrustClass { get; init; }

        public string? OfficialSourceRegistrationId { get; init; }

        public string? OfficialSnapshotId { get; init; }

        internal DocumentVersion ToDomain() =>
            new(
                new DocumentId(Id),
                new DocumentVersionNumber(Version),
                new DatabaseProductId(DatabaseProductId),
                new DatabaseProductRevision(DatabaseProductRevision),
                Enum.Parse<DocumentFormat>(Format, ignoreCase: false),
                ParseLanguage(ContentLanguage),
                ParseStatus(Status),
                new ContentObjectId(ContentObjectId),
                ByteLength,
                MediaType,
                new SourceAdapterId(SourceAdapterId),
                Enum.Parse<SourceTrustClass>(SourceTrustClass, ignoreCase: false),
                OfficialSourceRegistrationId is null
                    ? null
                    : new OfficialSourceRegistrationId(OfficialSourceRegistrationId),
                OfficialSnapshotId is null
                    ? null
                    : new OfficialSnapshotId(OfficialSnapshotId),
                SourceDeclaredLanguage is null
                    ? null
                    : new SourceDeclaredLanguage(SourceDeclaredLanguage));
    }

    private sealed class OfficialSourceRegistrationPayload
    {
        public required string RegistrationId { get; init; }

        public long Revision { get; init; }

        public required string DatabaseProductId { get; init; }

        public required string DocumentId { get; init; }

        public required string SourceAdapterId { get; init; }

        public required string CanonicalHttpsUrl { get; init; }

        public required string Status { get; init; }

        internal OfficialSourceRegistration ToDomain() =>
            new(
                new OfficialSourceRegistrationId(RegistrationId),
                new SourceRegistrationRevision(Revision),
                new DatabaseProductId(DatabaseProductId),
                new DocumentId(DocumentId),
                new SourceAdapterId(SourceAdapterId),
                CanonicalHttpsUrl,
                ParseStatus(Status));
    }

    private sealed class GenerationActivationPayload
    {
        public long ExpectedCurrentRevision { get; init; }

        public int PreviousGenerationRetentionDays { get; init; }

        public required GenerationManifestPayload Manifest { get; init; }

        public required DocumentActivationEvidenceBindingPayload[] EvidenceBindings { get; init; }
    }

    private sealed class GenerationManifestPayload
    {
        public int ManifestSchemaVersion { get; init; }

        public long CorpusRevision { get; init; }

        public long CatalogueRevision { get; init; }

        public required string ActiveDocumentSetDigest { get; init; }

        public required string SourceBindingSetDigest { get; init; }

        public required string IndexCompatibilityKey { get; init; }

        public required string GenerationSpecDigest { get; init; }

        public long ChunkCount { get; init; }

        public long VectorCount { get; init; }

        public required string LogicalArtifactDigest { get; init; }

        public required string GenerationContentDigest { get; init; }

        public required string IndexGenerationId { get; init; }

        internal FinalisedIndexGenerationManifest ToDomain(CorpusId corpusId) =>
            new(
                ManifestSchemaVersion,
                corpusId,
                new CorpusRevision(CorpusRevision),
                new CatalogueRevision(CatalogueRevision),
                new ActiveDocumentSetDigest(ActiveDocumentSetDigest),
                new SourceBindingSetDigest(SourceBindingSetDigest),
                new IndexCompatibilityKey(IndexCompatibilityKey),
                new GenerationSpecDigest(GenerationSpecDigest),
                ChunkCount,
                VectorCount,
                new LogicalArtifactDigest(LogicalArtifactDigest),
                new GenerationContentDigest(GenerationContentDigest),
                new IndexGenerationId(IndexGenerationId));
    }

    private sealed class DocumentActivationEvidenceBindingPayload
    {
        public required DocumentBindingPayload Binding { get; init; }

        public required string SourceContentObjectId { get; init; }

        public int RightsSchemaVersion { get; init; }

        public required DocumentRightDecisionPayload[] RightsDecisions { get; init; }

        public string? RenderManifestId { get; init; }

        internal DocumentActivationEvidenceBinding ToDomain()
        {
            if (RightsSchemaVersion != DocumentRightsEligibilityRecordV1.CurrentSchemaVersion)
            {
                throw new InvalidDataException(
                    "An activation rights snapshot must use schema version 1.");
            }

            var binding = Binding.ToDomain();
            var rights = new DocumentRightsEligibilityRecordV1(
                binding.DocumentId,
                binding.DocumentVersion,
                RightsDecisions.Select(decision => decision.ToDomain()));
            return new DocumentActivationEvidenceBinding(
                binding,
                new ContentObjectId(SourceContentObjectId),
                rights,
                RenderManifestId is null ? null : new RenderManifestId(RenderManifestId));
        }
    }

    private sealed class DocumentRightDecisionPayload
    {
        public required string Right { get; init; }

        public required string State { get; init; }

        public required string EvidenceReference { get; init; }

        internal DocumentRightDecision ToDomain() =>
            new(
                Enum.Parse<DocumentRight>(Right, ignoreCase: false),
                Enum.Parse<DocumentRightDecisionState>(State, ignoreCase: false),
                new DocumentRightsEvidenceReference(EvidenceReference));
    }

    private sealed class DocumentBindingPayload
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
                Enum.Parse<DocumentFormat>(DocumentFormat, ignoreCase: false),
                new SourceAdapterId(SourceAdapterId),
                Enum.Parse<SourceTrustClass>(SourceTrustClass, ignoreCase: false),
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

    private static CatalogueItemStatus ParseStatus(string value) =>
        Enum.Parse<CatalogueItemStatus>(value, ignoreCase: false);

    private static DocumentContentLanguage ParseLanguage(string value) => new(value);

    private static class CatalogueCommandPolicy
    {
        internal static void Validate(
            string command,
            string targetId,
            long? targetVersion,
            CatalogueSnapshot? current,
            CatalogueSnapshot proposed,
            long expectedRevision)
        {
            if (expectedRevision < 0 ||
                proposed.Revision.Value != expectedRevision + 1)
            {
                throw new InvalidDataException(
                    "The catalogue command did not target the current revision.");
            }

            var exactReplay = current?.Revision == proposed.Revision;

            if (!exactReplay && (current?.Revision.Value ?? 0) != expectedRevision)
            {
                throw new InvalidDataException(
                    "The catalogue command did not target the current revision.");
            }

            var currentProducts = current?.DatabaseProducts.ToDictionary(product => product.Id) ??
                new Dictionary<DatabaseProductId, DatabaseProduct>();
            var proposedProducts = proposed.DatabaseProducts.ToDictionary(product => product.Id);
            var changedProducts = currentProducts.Keys.Union(proposedProducts.Keys)
                .Where(id => !ProductEquivalent(
                    currentProducts.GetValueOrDefault(id),
                    proposedProducts.GetValueOrDefault(id)))
                .ToArray();
            var currentDocuments = current?.DocumentVersions
                .GroupBy(document => document.Id)
                .ToDictionary(group => group.Key, group => group.ToArray()) ??
                new Dictionary<DocumentId, DocumentVersion[]>();
            var proposedDocuments = proposed.DocumentVersions
                .GroupBy(document => document.Id)
                .ToDictionary(group => group.Key, group => group.ToArray());
            var changedDocuments = currentDocuments.Keys.Union(proposedDocuments.Keys)
                .Where(id => !DocumentSetEquivalent(
                    currentDocuments.GetValueOrDefault(id),
                    proposedDocuments.GetValueOrDefault(id)))
                .ToArray();
            ValidateCategories(command, current, proposed, targetId);
            if (current is not null)
            {
                EnsureActiveDocumentOwnership(current);
            }

            EnsureActiveDocumentOwnership(proposed);

            if (exactReplay)
            {
                if (changedProducts.Length != 0 || changedDocuments.Length != 0)
                {
                    throw new InvalidDataException(
                        "An idempotent catalogue replay must preserve the exact snapshot.");
                }

                return;
            }

            if (command.EndsWith("-database", StringComparison.Ordinal))
            {
                if (targetVersion is not null)
                {
                    throw new InvalidDataException(
                        "A database command cannot declare a document version target.");
                }

                ValidateDatabaseCommand(
                    command,
                    new DatabaseProductId(targetId),
                    currentProducts,
                    proposedProducts,
                    changedProducts,
                    changedDocuments,
                    currentDocuments,
                    proposedDocuments);
            }
            else
            {
                if (targetVersion is null or <= 0)
                {
                    throw new InvalidDataException(
                        "A document command requires its exact positive version target.");
                }

                ValidateDocumentCommand(
                    command,
                    new DocumentId(targetId),
                    new DocumentVersionNumber(targetVersion.Value),
                    proposedProducts,
                    changedProducts,
                    currentDocuments,
                    proposedDocuments,
                    changedDocuments);
            }
        }

        private static void ValidateCategories(
            string command,
            CatalogueSnapshot? current,
            CatalogueSnapshot proposed,
            string targetId)
        {
            var proposedCategories = proposed.DatabaseCategories.ToDictionary(
                category => category.Id);
            var referencedCategories = proposed.DatabaseProducts
                .SelectMany(product => product.CategoryIds)
                .ToHashSet();

            if (!referencedCategories.SetEquals(proposedCategories.Keys))
            {
                throw new InvalidDataException(
                    "Every catalogue category must remain assigned in the same snapshot.");
            }

            if (current is null)
            {
                if (!string.Equals(command, "add-database", StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Only add-database can create the initial catalogue categories.");
                }

                var product = proposed.DatabaseProducts.SingleOrDefault(item =>
                    string.Equals(item.Id.Value, targetId, StringComparison.Ordinal)) ??
                    throw new InvalidDataException(
                        "The initial database target is absent from the proposed catalogue.");
                var referenced = product.CategoryIds.ToHashSet();

                if (proposedCategories.Keys.Any(categoryId => !referenced.Contains(categoryId)))
                {
                    throw new InvalidDataException(
                        "Initial category creation must be limited to the added database.");
                }

                return;
            }

            var currentCategories = current.DatabaseCategories.ToDictionary(category => category.Id);

            foreach (var category in currentCategories)
            {
                if (!proposedCategories.TryGetValue(category.Key, out var proposedCategory) ||
                    !string.Equals(
                        category.Value.DisplayName,
                        proposedCategory.DisplayName,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "An administrative command cannot remove or rename a category.");
                }
            }

            var added = proposedCategories.Keys.Except(currentCategories.Keys).ToArray();

            if (added.Length == 0)
            {
                return;
            }

            if (!string.Equals(command, "add-database", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Only add-database can introduce categories used by its target.");
            }

            var addedProduct = proposed.DatabaseProducts.SingleOrDefault(item =>
                string.Equals(item.Id.Value, targetId, StringComparison.Ordinal)) ??
                throw new InvalidDataException("The added database target is absent.");
            var targetCategories = addedProduct.CategoryIds.ToHashSet();

            if (added.Any(categoryId => !targetCategories.Contains(categoryId)))
            {
                throw new InvalidDataException(
                    "A new category must be assigned to the added database target.");
            }
        }

        private static void ValidateDatabaseCommand(
            string command,
            DatabaseProductId targetId,
            IReadOnlyDictionary<DatabaseProductId, DatabaseProduct> current,
            IReadOnlyDictionary<DatabaseProductId, DatabaseProduct> proposed,
            DatabaseProductId[] changedProducts,
            DocumentId[] changedDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> currentDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> proposedDocuments)
        {
            if (changedProducts.Length != 1)
            {
                throw new InvalidDataException(
                    "A database command must change exactly one logical database.");
            }

            var id = changedProducts[0];

            if (id != targetId)
            {
                throw new InvalidDataException(
                    "A database command changed an identity other than its declared target.");
            }

            var before = current.GetValueOrDefault(id);
            var after = proposed.GetValueOrDefault(id) ??
                throw new InvalidDataException("Physical database removal is forbidden.");

            switch (command)
            {
                case "add-database" when
                    before is null && after.Revision.Value == 1 &&
                    after.Status == CatalogueItemStatus.Candidate &&
                    changedDocuments.Length == 0:
                    return;
                case "version-database" when
                    before is not null &&
                    before.Status is CatalogueItemStatus.Candidate or
                        CatalogueItemStatus.Deactivated &&
                    after.Revision.Value == before.Revision.Value + 1 &&
                    after.Status == CatalogueItemStatus.Candidate:
                    EnsureDatabaseDocumentRebinding(
                        id,
                        before,
                        after,
                        changedDocuments,
                        currentDocuments,
                        proposedDocuments,
                        deactivateActiveDocuments: false);
                    return;
                case "activate-database":
                    EnsureStatusTransition(before, after, CatalogueItemStatus.Active);
                    EnsureDatabaseDocumentStatusTransition(
                        id,
                        changedDocuments,
                        currentDocuments,
                        proposedDocuments,
                        fromStatuses:
                        [CatalogueItemStatus.Candidate, CatalogueItemStatus.Deactivated],
                        toStatus: CatalogueItemStatus.Active,
                        requireEveryMatchingStatus: false);
                    return;
                case "deactivate-database":
                    EnsureStatusTransition(before, after, CatalogueItemStatus.Deactivated);
                    EnsureDatabaseDocumentStatusTransition(
                        id,
                        changedDocuments,
                        currentDocuments,
                        proposedDocuments,
                        fromStatuses: [CatalogueItemStatus.Active],
                        toStatus: CatalogueItemStatus.Deactivated,
                        requireEveryMatchingStatus: true);
                    return;
                case "remove-database":
                    EnsureStatusTransition(before, after, CatalogueItemStatus.Removed);
                    EnsureNoActiveDocuments(id, currentDocuments);

                    if (changedDocuments.Length != 0)
                    {
                        throw new InvalidDataException(
                            "Removing a database cannot change its documents.");
                    }

                    return;
                default:
                    throw new InvalidDataException(
                        "The proposed database transition does not match the command.");
            }
        }

        private static void ValidateDocumentCommand(
            string command,
            DocumentId targetId,
            DocumentVersionNumber targetVersion,
            Dictionary<DatabaseProductId, DatabaseProduct> proposedProducts,
            DatabaseProductId[] changedProducts,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> currentDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> proposedDocuments,
            DocumentId[] changedDocuments)
        {
            if (changedDocuments.Length != 1)
            {
                throw new InvalidDataException(
                    "A document command must change exactly one logical document.");
            }

            var id = changedDocuments[0];

            if (id != targetId)
            {
                throw new InvalidDataException(
                    "A document command changed an identity other than its declared target.");
            }

            var before = currentDocuments.GetValueOrDefault(id) ?? [];
            var after = proposedDocuments.GetValueOrDefault(id) ??
                throw new InvalidDataException("Physical document removal is forbidden.");
            var latest = after.MaxBy(document => document.Version.Value)!;
            var previousLatest = before.MaxBy(document => document.Version.Value);

            if (latest.Version != targetVersion)
            {
                throw new InvalidDataException(
                    "A document command changed a version other than its declared target.");
            }
            if (changedProducts.Length != 0)
            {
                throw new InvalidDataException(
                    "A document command cannot change a database product.");
            }

            if (!proposedProducts.TryGetValue(latest.DatabaseProductId, out var owner) ||
                owner.Status == CatalogueItemStatus.Removed ||
                command == "activate-document" && owner.Status != CatalogueItemStatus.Active)
            {
                throw new InvalidDataException(
                    "The document command is incompatible with its database lifecycle.");
            }

            switch (command)
            {
                case "add-document" when
                    before.Length == 0 && after.Length == 1 &&
                    latest.Version.Value == 1 &&
                    latest.Status == CatalogueItemStatus.Candidate:
                    return;
                case "version-document" when
                    before.Length > 0 &&
                    previousLatest!.Status != CatalogueItemStatus.Removed &&
                    latest.DatabaseProductId == previousLatest.DatabaseProductId &&
                    latest.DatabaseProductRevision ==
                        previousLatest.DatabaseProductRevision &&
                    latest.Version.Value == before.Max(document => document.Version.Value) + 1 &&
                    latest.Status == CatalogueItemStatus.Candidate &&
                    ExistingDocumentVersionsUnchanged(before, after):
                    return;
                case "activate-document":
                    EnsureDocumentStatus(
                        before,
                        after,
                        targetVersion,
                        CatalogueItemStatus.Active);
                    return;
                case "deactivate-document":
                    EnsureDocumentStatus(
                        before,
                        after,
                        targetVersion,
                        CatalogueItemStatus.Deactivated);
                    return;
                case "remove-document":
                    EnsureDocumentStatus(
                        before,
                        after,
                        targetVersion,
                        CatalogueItemStatus.Removed);
                    return;
                default:
                    throw new InvalidDataException(
                        "The proposed document transition does not match the command.");
            }
        }

        private static void EnsureStatusTransition(
            DatabaseProduct? before,
            DatabaseProduct after,
            CatalogueItemStatus requiredStatus)
        {
            if (before is null || after.Status != requiredStatus ||
                after.Revision != before.Revision ||
                !string.Equals(before.DisplayName, after.DisplayName, StringComparison.Ordinal) ||
                !before.CategoryIds.OrderBy(id => id.Value, StringComparer.Ordinal)
                    .SequenceEqual(
                        after.CategoryIds.OrderBy(id => id.Value, StringComparer.Ordinal)))
            {
                throw new InvalidDataException("The database lifecycle transition is invalid.");
            }

            CatalogueLifecycle.EnsureTransition(before.Status, after.Status);
        }

        private static void EnsureDocumentStatus(
            DocumentVersion[] before,
            DocumentVersion[] after,
            DocumentVersionNumber targetVersion,
            CatalogueItemStatus requiredStatus)
        {
            if (before.Length != after.Length)
            {
                throw new InvalidDataException(
                    "A document status command cannot add or remove versions.");
            }

            var previous = before.SingleOrDefault(document =>
                document.Version == targetVersion) ??
                throw new InvalidDataException(
                    "A status command cannot create a document version.");
            var proposed = after.SingleOrDefault(document =>
                document.Version == targetVersion) ??
                throw new InvalidDataException(
                    "A status command must preserve its target document version.");

            if (proposed.Status != requiredStatus ||
                !DocumentEquivalentExceptStatus(previous, proposed) ||
                before.Where(document => document.Version != targetVersion)
                    .Select(DocumentProjection)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(
                        after.Where(document => document.Version != targetVersion)
                            .Select(DocumentProjection)
                            .OrderBy(value => value, StringComparer.Ordinal)) is false)
            {
                throw new InvalidDataException("The document status does not match the command.");
            }

            CatalogueLifecycle.EnsureTransition(previous.Status, proposed.Status);
        }

        private static void EnsureDatabaseDocumentRebinding(
            DatabaseProductId targetId,
            DatabaseProduct beforeProduct,
            DatabaseProduct afterProduct,
            DocumentId[] changedDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> currentDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> proposedDocuments,
            bool deactivateActiveDocuments)
        {
            var expectedChanged = currentDocuments
                .Where(pair => pair.Value.Any(document =>
                    document.DatabaseProductId == targetId))
                .Select(pair => pair.Key)
                .OrderBy(id => id.Value, StringComparer.Ordinal)
                .ToArray();

            if (!expectedChanged.SequenceEqual(
                    changedDocuments.OrderBy(id => id.Value, StringComparer.Ordinal)))
            {
                throw new InvalidDataException(
                    "A database command must rebind exactly its existing documents.");
            }

            foreach (var documentId in expectedChanged)
            {
                var before = currentDocuments[documentId];
                var after = proposedDocuments.GetValueOrDefault(documentId) ??
                    throw new InvalidDataException(
                        "A database command cannot remove a document.");

                if (before.Length != after.Length)
                {
                    throw new InvalidDataException(
                        "A database command cannot add or remove document versions.");
                }

                foreach (var prior in before)
                {
                    var rebound = after.SingleOrDefault(candidate =>
                        candidate.Version == prior.Version) ??
                        throw new InvalidDataException(
                            "A database command must preserve document version identities.");

                    var statusMatches = deactivateActiveDocuments &&
                        prior.Status == CatalogueItemStatus.Active
                            ? rebound.Status == CatalogueItemStatus.Deactivated &&
                                DocumentEquivalentExceptDatabaseRevisionAndStatus(
                                    prior,
                                    rebound)
                            : DocumentEquivalentExceptDatabaseRevision(prior, rebound);

                    if (prior.DatabaseProductRevision != beforeProduct.Revision ||
                        rebound.DatabaseProductRevision != afterProduct.Revision ||
                        !statusMatches)
                    {
                        throw new InvalidDataException(
                            "A database command changed an unauthorised document field.");
                    }
                }
            }
        }

        private static void EnsureDatabaseDocumentStatusTransition(
            DatabaseProductId targetId,
            DocumentId[] changedDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> currentDocuments,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> proposedDocuments,
            IReadOnlyCollection<CatalogueItemStatus> fromStatuses,
            CatalogueItemStatus toStatus,
            bool requireEveryMatchingStatus)
        {
            var eligible = currentDocuments
                .Where(pair => pair.Value.Any(document =>
                    document.DatabaseProductId == targetId &&
                    fromStatuses.Contains(document.Status)))
                .Select(pair => pair.Key)
                .ToHashSet();

            if (changedDocuments.Length == 0 ||
                changedDocuments.Any(documentId => !eligible.Contains(documentId)) ||
                requireEveryMatchingStatus && !eligible.SetEquals(changedDocuments))
            {
                throw new InvalidDataException(
                    "The database lifecycle command changed the wrong document set.");
            }

            foreach (var documentId in changedDocuments)
            {
                var before = currentDocuments[documentId];
                var after = proposedDocuments.GetValueOrDefault(documentId) ??
                    throw new InvalidDataException(
                        "A database lifecycle command cannot remove a document.");

                if (before.Length != after.Length)
                {
                    throw new InvalidDataException(
                        "A database lifecycle command cannot add or remove versions.");
                }

                foreach (var prior in before)
                {
                    var updated = after.SingleOrDefault(candidate =>
                        candidate.Version == prior.Version) ??
                        throw new InvalidDataException(
                            "A database lifecycle command must preserve version identities.");
                    var shouldTransition = prior.DatabaseProductId == targetId &&
                        fromStatuses.Contains(prior.Status);

                    if (shouldTransition)
                    {
                        if (updated.Status != toStatus ||
                            !DocumentEquivalentExceptStatus(prior, updated))
                        {
                            throw new InvalidDataException(
                                "A database lifecycle command changed an unauthorised document field.");
                        }
                    }
                    else if (!string.Equals(
                        DocumentProjection(prior),
                        DocumentProjection(updated),
                        StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            "A database lifecycle command changed an unrelated document version.");
                    }
                }
            }
        }

        private static void EnsureActiveDocumentOwnership(CatalogueSnapshot snapshot)
        {
            var products = snapshot.DatabaseProducts.ToDictionary(product => product.Id);

            if (snapshot.DocumentVersions.Any(document =>
                    document.Status == CatalogueItemStatus.Active &&
                    (!products.TryGetValue(document.DatabaseProductId, out var product) ||
                        product.Status != CatalogueItemStatus.Active)))
            {
                throw new InvalidDataException(
                    "Every active document must belong to an active database product.");
            }
        }

        private static void EnsureNoActiveDocuments(
            DatabaseProductId targetId,
            IReadOnlyDictionary<DocumentId, DocumentVersion[]> documents)
        {
            if (documents.Values.SelectMany(value => value).Any(document =>
                    document.DatabaseProductId == targetId &&
                    document.Status == CatalogueItemStatus.Active))
            {
                throw new InvalidDataException(
                    "A database with active documents cannot be removed.");
            }
        }

        private static bool ExistingDocumentVersionsUnchanged(
            DocumentVersion[] before,
            DocumentVersion[] after) =>
            after.Length == before.Length + 1 &&
            before.Select(DocumentProjection)
                .OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(
                    after.Where(candidate => before.Any(previous =>
                            previous.Version == candidate.Version))
                        .Select(DocumentProjection)
                        .OrderBy(value => value, StringComparer.Ordinal));

        private static bool DocumentEquivalentExceptStatus(
            DocumentVersion left,
            DocumentVersion right) =>
            left.Id == right.Id &&
            left.Version == right.Version &&
            left.DatabaseProductId == right.DatabaseProductId &&
            left.DatabaseProductRevision == right.DatabaseProductRevision &&
            left.Format == right.Format &&
            left.ContentLanguage == right.ContentLanguage &&
            left.SourceDeclaredLanguage == right.SourceDeclaredLanguage &&
            left.ContentObjectId == right.ContentObjectId &&
            left.ByteLength == right.ByteLength &&
            string.Equals(left.MediaType, right.MediaType, StringComparison.Ordinal) &&
            left.SourceAdapterId == right.SourceAdapterId &&
            left.SourceTrustClass == right.SourceTrustClass &&
            left.OfficialSourceRegistrationId == right.OfficialSourceRegistrationId &&
            left.OfficialSnapshotId == right.OfficialSnapshotId;

        private static bool DocumentEquivalentExceptDatabaseRevision(
            DocumentVersion left,
            DocumentVersion right) =>
            left.Id == right.Id &&
            left.Version == right.Version &&
            left.DatabaseProductId == right.DatabaseProductId &&
            left.Format == right.Format &&
            left.ContentLanguage == right.ContentLanguage &&
            left.SourceDeclaredLanguage == right.SourceDeclaredLanguage &&
            left.Status == right.Status &&
            left.ContentObjectId == right.ContentObjectId &&
            left.ByteLength == right.ByteLength &&
            string.Equals(left.MediaType, right.MediaType, StringComparison.Ordinal) &&
            left.SourceAdapterId == right.SourceAdapterId &&
            left.SourceTrustClass == right.SourceTrustClass &&
            left.OfficialSourceRegistrationId == right.OfficialSourceRegistrationId &&
            left.OfficialSnapshotId == right.OfficialSnapshotId;

        private static bool DocumentEquivalentExceptDatabaseRevisionAndStatus(
            DocumentVersion left,
            DocumentVersion right) =>
            left.Id == right.Id &&
            left.Version == right.Version &&
            left.DatabaseProductId == right.DatabaseProductId &&
            left.Format == right.Format &&
            left.ContentLanguage == right.ContentLanguage &&
            left.SourceDeclaredLanguage == right.SourceDeclaredLanguage &&
            left.ContentObjectId == right.ContentObjectId &&
            left.ByteLength == right.ByteLength &&
            string.Equals(left.MediaType, right.MediaType, StringComparison.Ordinal) &&
            left.SourceAdapterId == right.SourceAdapterId &&
            left.SourceTrustClass == right.SourceTrustClass &&
            left.OfficialSourceRegistrationId == right.OfficialSourceRegistrationId &&
            left.OfficialSnapshotId == right.OfficialSnapshotId;

        private static bool ProductEquivalent(DatabaseProduct? left, DatabaseProduct? right) =>
            left is null && right is null ||
            left is not null && right is not null &&
            left.Revision == right.Revision &&
            left.Status == right.Status &&
            string.Equals(left.DisplayName, right.DisplayName, StringComparison.Ordinal) &&
            left.CategoryIds.OrderBy(id => id.Value, StringComparer.Ordinal)
                .SequenceEqual(right.CategoryIds.OrderBy(id => id.Value, StringComparer.Ordinal));

        private static bool DocumentSetEquivalent(
            DocumentVersion[]? left,
            DocumentVersion[]? right)
        {
            left ??= [];
            right ??= [];
            return left.OrderBy(document => document.Version.Value)
                .Select(DocumentProjection)
                .SequenceEqual(
                    right.OrderBy(document => document.Version.Value)
                        .Select(DocumentProjection));
        }

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
    }
}
