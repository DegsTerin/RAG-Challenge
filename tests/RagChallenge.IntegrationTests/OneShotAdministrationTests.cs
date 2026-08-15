// Purpose: Proves the accepted one-shot command allowlist, fail-closed host separation, bounded local input, lease ownership, idempotent catalogue commit and absence of HTTP administration.
using System.Diagnostics;
using System.Text.Json;

using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class OneShotAdministrationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 4, 16, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions PlanJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task RealActivationCommandRequiresAndPersistsCompleteEvidenceBindings()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var vectorStore = new SqliteVectorIndexStore(options);
        var contentStore = new ImmutableContentStore(options);
        var corpusId = new CorpusId("admin-corpus");
        var productId = new DatabaseProductId("admin-database");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("admin-document-csv");
        var documentVersion = new DocumentVersionNumber(1);
        var sourceAdapterId = new SourceAdapterId("admin-local-csv");
        var sourceBytes = System.Text.Encoding.UTF8.GetBytes("name,value\nfixture,1\n");
        await using var source = new MemoryStream(sourceBytes, writable: false);
        var content = await contentStore.PutAndVerifyAsync(new BoundedContentInput(
            source,
            sourceBytes.Length,
            ContentMediaType.TextCsv));
        var category = new DatabaseCategory(
            new DatabaseCategoryId("admin-category"),
            "Administration category");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Administration database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            content.ContentObjectId,
            content.ByteLength,
            ContentMediaType.TextCsv.Value,
            sourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        var catalogue = new CatalogueSnapshot(
            corpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await store.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("admin-activation-catalogue"),
                catalogue,
                ExpectedCurrentRevision: 0,
                Now))).Outcome);
        var binding = new DocumentBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            sourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        var compatibilityKey = new IndexCompatibilityKey(new string('a', 64));
        var candidate = new CandidateBuildId("admin-activation-candidate");
        await vectorStore.CreateCandidateAsync(
            candidate,
            corpusId,
            compatibilityKey,
            vectorDimensions: 2,
            expectedChunkCount: 1,
            Now);
        await vectorStore.AddChunksAsync(candidate, [new VectorChunkWrite(
            0,
            documentId,
            documentVersion,
            new LogicalArtifactDigest(new string('b', 64)),
            "synthetic activation evidence",
            new float[] { 1, 2 },
            DocumentContentLanguage.EnGb)]);
        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            corpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet([binding]).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet([binding]).Digest,
            compatibilityKey);
        var manifest = await vectorStore.FinaliseCandidateAsync(
            candidate,
            specification,
            Now);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await store.CommitGenerationAsync(new GenerationCommitRequest(
                new OperationId("admin-activation-generation"),
                candidate,
                manifest,
                [binding],
                Now))).Outcome);
        var input = new
        {
            expectedCurrentRevision = 0,
            previousGenerationRetentionDays = 14,
            manifest = new
            {
                manifest.ManifestSchemaVersion,
                corpusRevision = manifest.CorpusRevision.Value,
                catalogueRevision = manifest.CatalogueRevision.Value,
                activeDocumentSetDigest = manifest.ActiveDocumentSetDigest.Value,
                sourceBindingSetDigest = manifest.SourceBindingSetDigest.Value,
                indexCompatibilityKey = manifest.IndexCompatibilityKey.Value,
                generationSpecDigest = manifest.GenerationSpecDigest.Value,
                manifest.ChunkCount,
                manifest.VectorCount,
                logicalArtifactDigest = manifest.LogicalArtifactDigest.Value,
                generationContentDigest = manifest.GenerationContentDigest.Value,
                indexGenerationId = manifest.IndexGenerationId.Value,
            },
            evidenceBindings = new[]
            {
                new
                {
                    binding = new
                    {
                        databaseProductId = productId.Value,
                        databaseProductRevision = productRevision.Value,
                        documentId = documentId.Value,
                        documentVersion = documentVersion.Value,
                        documentFormat = DocumentFormat.Csv.ToString(),
                        sourceAdapterId = sourceAdapterId.Value,
                        sourceTrustClass = SourceTrustClass.LocalAuthorised.ToString(),
                        officialSourceRegistrationId = (string?)null,
                        officialSnapshotId = (string?)null,
                        sourceObservationId = (string?)null,
                    },
                    sourceContentObjectId = content.ContentObjectId.Value,
                    rightsSchemaVersion = 1,
                    rightsDecisions = Enum.GetValues<DocumentRight>().Select(right => new
                    {
                        right = right.ToString(),
                        state = DocumentRightDecisionState.Permitted.ToString(),
                        evidenceReference = $"admin-rights-{right}",
                    }).ToArray(),
                    renderManifestId = (string?)null,
                },
            },
        };
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "activate.json"),
            JsonSerializer.Serialize(input, PlanJsonOptions));
        var result = await RunAsync(
            MutationArguments(
                "activate-generation",
                "admin-activation-operation",
                "activate.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('a', 64)),
            new SqliteAdministrationLeaseManager(options),
            new SqliteAdministrativeCommandExecutor(store),
            new SqliteAdministrationCommandJournal(options));
        var active = Assert.IsType<CorpusActivationRecord>(
            await store.ReadActiveActivationAsync(corpusId));

        Assert.Equal((int)AdministrationExitCode.Success, result.ExitCode);
        Assert.True(active.HasCompleteEvidenceBindings);
        Assert.Equal(10, active.EvidenceBindings[0].Rights.Decisions.Count);
        Assert.Equal(1, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));
    }

    [Fact]
    public async Task ExactCommandSetIsOneShotOnlyAndNeverMappedToHttp()
    {
        string[] expected =
        [
            "activate-database",
            "activate-document",
            "activate-generation",
            "add-database",
            "add-document",
            "build-index",
            "deactivate-database",
            "deactivate-document",
            "import-local",
            "register-official-source",
            "remove-database",
            "remove-document",
            "render-document",
            "rollback-generation",
            "status",
            "synchronise-official",
            "version-database",
            "version-document",
        ];

        Assert.Equal(
            expected,
            AdministrativeCommands.Allowed.Order(StringComparer.Ordinal));
        Assert.All(expected, command => Assert.True(
            OneShotAdministrationHost.IsAdministrationMode(["admin", command])));

        await using var app = SetupHost.Build([]);
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(routes, route =>
            route.Contains("admin", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LocalImportPublishesOnlyTheExpectedBoundedFile()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var bytes = "%PDF-1.5\nsynthetic local import\n%%EOF\n"u8.ToArray();
        var sourcePath = Path.Combine(root.InputRoot, "oracle-19c.pdf");
        await File.WriteAllBytesAsync(sourcePath, bytes);
        var sha256 = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "import.json"),
            JsonSerializer.Serialize(new
            {
                relativePath = "oracle-19c.pdf",
                maximumByteLength = 1024,
                mediaType = "application/pdf",
                expectedSha256 = sha256,
            }));
        var ports = new AdministrativeMaterialisationPorts(
            LocalInputRoot: root.InputRoot);
        var executor = AdministrativeMaterialisationComposition.CreateExecutor(
            options,
            new SqliteControlPlaneStore(options),
            ports);

        var result = await RunAsync(
            MutationArguments("import-local", "local-import-v1", "import.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('a', 64)),
            new RecordingLeaseManager(),
            executor);

        Assert.Equal((int)AdministrationExitCode.Success, result.ExitCode);
        using var output = JsonDocument.Parse(result.Output);
        var payload = output.RootElement.GetProperty("resultPayload");
        Assert.Equal(sha256, payload.GetProperty("contentObjectId").GetString());
        Assert.Equal(bytes.Length, payload.GetProperty("byteLength").GetInt64());
        Assert.Equal("application/pdf", payload.GetProperty("mediaType").GetString());
        Assert.True(File.Exists(Path.Combine(
            options.ContentStoreRoot,
            "objects",
            sha256[..2],
            $"{sha256}.bin")));
    }

    [Fact]
    public async Task LocalImportRejectsTraversalBeforeContentPublication()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var bytes = "%PDF-1.5\nexternal synthetic source\n%%EOF\n"u8.ToArray();
        var outsidePath = Path.Combine(root.Root, "outside.pdf");
        await File.WriteAllBytesAsync(outsidePath, bytes);

        var result = await RunLocalImportAsync(
            root,
            options,
            "local-import-traversal",
            new
            {
                relativePath = "../outside.pdf",
                maximumByteLength = 1024,
                mediaType = "application/pdf",
                expectedSha256 = Sha256(bytes),
            });

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");
        Assert.Equal(bytes, await File.ReadAllBytesAsync(outsidePath));
        AssertNoContentResidue(options);
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("size")]
    public async Task LocalImportRejectsHashAndByteLimitDriftWithoutResidue(
        string divergence)
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var bytes = "%PDF-1.5\nidentity drift\n%%EOF\n"u8.ToArray();
        await File.WriteAllBytesAsync(
            Path.Combine(root.InputRoot, "oracle-19c.pdf"),
            bytes);
        var result = await RunLocalImportAsync(
            root,
            options,
            $"local-import-{divergence}",
            new
            {
                relativePath = "oracle-19c.pdf",
                maximumByteLength = divergence == "size" ? bytes.Length - 1 : bytes.Length,
                mediaType = "application/pdf",
                expectedSha256 = divergence == "hash"
                    ? new string('a', 64)
                    : Sha256(bytes),
            });

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");
        AssertNoContentResidue(options);
    }

    [Theory]
    [InlineData("oracle-19c.pdf", "application/octet-stream")]
    [InlineData("oracle-19c.csv", "application/pdf")]
    public async Task LocalImportRejectsUnsupportedOrMismatchedMediaType(
        string fileName,
        string mediaType)
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var bytes = "synthetic media type drift"u8.ToArray();
        await File.WriteAllBytesAsync(Path.Combine(root.InputRoot, fileName), bytes);

        var result = await RunLocalImportAsync(
            root,
            options,
            $"local-import-media-{Path.GetExtension(fileName)[1..]}",
            new
            {
                relativePath = fileName,
                maximumByteLength = bytes.Length,
                mediaType,
                expectedSha256 = Sha256(bytes),
            });

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");
        AssertNoContentResidue(options);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LocalImportRejectsFileAndDirectoryReparsePoints(bool markDirectory)
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var relativeDirectory = "linked-source";
        var directory = Path.Combine(root.InputRoot, relativeDirectory);
        Directory.CreateDirectory(directory);
        var sourcePath = Path.Combine(directory, "oracle-19c.pdf");
        var bytes = "%PDF-1.5\nreparse boundary\n%%EOF\n"u8.ToArray();
        await File.WriteAllBytesAsync(sourcePath, bytes);
        var markedPath = markDirectory ? directory : sourcePath;

        FileAttributes ReadAttributes(string path)
        {
            var attributes = File.GetAttributes(path);
            return string.Equals(
                    Path.GetFullPath(path),
                    Path.GetFullPath(markedPath),
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal)
                ? attributes | FileAttributes.ReparsePoint
                : attributes;
        }

        var result = await RunLocalImportAsync(
            root,
            options,
            markDirectory
                ? "local-import-directory-reparse"
                : "local-import-file-reparse",
            new
            {
                relativePath = $"{relativeDirectory}/oracle-19c.pdf",
                maximumByteLength = bytes.Length,
                mediaType = "application/pdf",
                expectedSha256 = Sha256(bytes),
            },
            ReadAttributes);

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");
        Assert.Equal(bytes, await File.ReadAllBytesAsync(sourcePath));
        AssertNoContentResidue(options);
    }

    [Fact]
    public async Task LocalImportRejectsAnActualFileReparsePointWhenSupported()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var bytes = "%PDF-1.5\nactual reparse boundary\n%%EOF\n"u8.ToArray();
        var outsidePath = Path.Combine(root.Root, "outside-reparse.pdf");
        var linkedPath = Path.Combine(root.InputRoot, "oracle-19c.pdf");
        await File.WriteAllBytesAsync(outsidePath, bytes);

        try
        {
            _ = File.CreateSymbolicLink(linkedPath, outsidePath);
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or IOException or
                PlatformNotSupportedException)
        {
            // The deterministic attribute seam above still covers both reparse branches.
            return;
        }

        var result = await RunLocalImportAsync(
            root,
            options,
            "local-import-actual-reparse",
            new
            {
                relativePath = "oracle-19c.pdf",
                maximumByteLength = bytes.Length,
                mediaType = "application/pdf",
                expectedSha256 = Sha256(bytes),
            });

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");
        Assert.Equal(bytes, await File.ReadAllBytesAsync(outsidePath));
        AssertNoContentResidue(options);
    }

    [Fact]
    public async Task DisabledConfigurationStopsBeforeIdentityLeaseOrExecution()
    {
        var identity = new StubIdentity("os-sha256:" + new string('a', 64));
        var lease = new RecordingLeaseManager();
        var executor = new RecordingExecutor();

        var result = await RunAsync(
            StatusArguments("disabled-operation"),
            Configuration(enabled: false),
            identity,
            lease,
            executor);

        Assert.Equal(
            (int)AdministrationExitCode.ConfigurationOrAuthorityDenied,
            result.ExitCode);
        Assert.Equal(0, identity.CallCount);
        Assert.Empty(lease.Acquisitions);
        Assert.Empty(executor.Commands);
        Assert.Contains("CH_ADMIN_DISABLED", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AllMutatingCommandsRequireTheSameGovernedLeaseAndInputPath()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "plan.json"), "{}");
        var configuration = Configuration(
            enabled: true,
            root.InputRoot,
            root.StoreRoot);

        foreach (var command in AdministrativeCommands.Allowed.Where(
                     AdministrativeCommands.IsMutation))
        {
            var lease = new RecordingLeaseManager();
            var executor = new RecordingExecutor();
            var result = await RunAsync(
                MutationArguments(command, $"operation-{command}", "plan.json"),
                configuration,
                new StubIdentity("os-sha256:" + new string('b', 64)),
                lease,
                executor);

            Assert.Equal((int)AdministrationExitCode.Success, result.ExitCode);
            Assert.Single(lease.Acquisitions);
            Assert.Single(lease.Releases);
            var routed = Assert.Single(executor.Commands);
            Assert.Equal(command, routed.Command);
            Assert.Equal(
                "os-sha256:" + new string('b', 64),
                routed.AuditContext.ActorIdentifier);
            Assert.Equal(
                "Execute a bounded synthetic administration test.",
                routed.AuditContext.Reason);
            Assert.Matches("^[0-9a-f]{64}$", executor.Commands[0].InputSha256!);
        }
    }

    [Fact]
    public async Task SuccessfulOneShotOutputPreservesSanitisedResultPayload()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "render.json"), "{}");
        var payload = JsonSerializer.SerializeToElement(new
        {
            renderManifestId = "rendermanifest-" + new string('a', 64),
        });
        var executor = new RecordingExecutor { ResultPayload = payload };

        var result = await RunAsync(
            MutationArguments(
                "render-document",
                "render-result-payload-operation",
                "render.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('b', 64)),
            new RecordingLeaseManager(),
            executor);
        using var output = JsonDocument.Parse(result.Output);

        Assert.Equal((int)AdministrationExitCode.Success, result.ExitCode);
        Assert.Equal(
            payload.GetProperty("renderManifestId").GetString(),
            output.RootElement.GetProperty("resultPayload")
                .GetProperty("renderManifestId").GetString());
    }

    [Fact]
    public async Task LeaseConflictPreventsCommandExecution()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "plan.json"), "{}");
        var lease = new RecordingLeaseManager
        {
            NextOutcome = AdministrationLeaseOutcome.Conflict,
        };
        var executor = new RecordingExecutor();
        var journal = new RecordingJournal();

        var result = await RunAsync(
            MutationArguments("add-database", "operation-conflict", "plan.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('c', 64)),
            lease,
            executor,
            journal);
        lease.NextOutcome = AdministrationLeaseOutcome.Acquired;
        var replay = await RunAsync(
            MutationArguments("add-database", "operation-conflict", "plan.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('c', 64)),
            lease,
            executor,
            journal,
            () => Now.AddMinutes(5));

        Assert.Equal((int)AdministrationExitCode.Conflict, result.ExitCode);
        Assert.Equal((int)AdministrationExitCode.Conflict, replay.ExitCode);
        Assert.Empty(executor.Commands);
        Assert.Single(lease.Acquisitions);
        Assert.Empty(lease.Releases);
    }

    [Fact]
    public async Task LeaseReleaseFailureCannotReplaceTheDurableCommandResult()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "plan.json"), "{}");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('9', 64));
        var lease = new RecordingLeaseManager { FailRelease = true };
        var executor = new RecordingExecutor();
        var journal = new RecordingJournal();
        var arguments = MutationArguments(
            "add-database",
            "operation-release-failure",
            "plan.json");

        var first = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal);
        var replay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(1));

        Assert.Equal((int)AdministrationExitCode.Success, first.ExitCode);
        Assert.Equal((int)AdministrationExitCode.Success, replay.ExitCode);
        Assert.Single(executor.Commands);
        Assert.Single(lease.Acquisitions);
        Assert.Equal(2, lease.Releases.Count);
    }

    [Fact]
    public async Task RealCatalogueCommandCommitsOnceAndReplaysIdempotently()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        const string input = """
            {
              "targetId": "database-one",
              "expectedCurrentRevision": 0,
              "revision": 1,
              "categories": [
                { "id": "relational", "displayName": "Relational databases" }
              ],
              "databaseProducts": [
                {
                  "id": "database-one",
                  "revision": 1,
                  "displayName": "Database One",
                  "status": "Candidate",
                  "categoryIds": ["relational"]
                }
              ],
              "documentVersions": []
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "add.json"), input);
        var store = new SqliteControlPlaneStore(options);
        var lease = new SqliteAdministrationLeaseManager(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var arguments = MutationArguments(
            "add-database",
            "operation-add-database",
            "add.json");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('d', 64));

        var journal = new SqliteAdministrationCommandJournal(options);
        var first = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal);
        var replay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(10));
        var mismatchedReplay = await RunAsync(
            MutationArguments(
                "version-database",
                "operation-add-database",
                "add.json"),
            configuration,
            identity,
            lease,
            executor,
            journal);

        Assert.Equal((int)AdministrationExitCode.Success, first.ExitCode);
        Assert.Equal((int)AdministrationExitCode.Success, replay.ExitCode);
        Assert.Equal(
            (int)AdministrationExitCode.Conflict,
            mismatchedReplay.ExitCode);
        Assert.Contains("CH_ADMIN_APPLIED", first.Output, StringComparison.Ordinal);
        Assert.Contains("CH_ADMIN_APPLIED", replay.Output, StringComparison.Ordinal);
        var current = await store.ReadCurrentCatalogueAsync(new CorpusId("admin-corpus"));
        Assert.NotNull(current);
        Assert.Equal(1, current.Revision.Value);
        Assert.Equal("database-one", Assert.Single(current.DatabaseProducts).Id.Value);
        Assert.Equal(1, await ScalarAsync(options, "SELECT COUNT(*) FROM admin_operations;"));
        Assert.Equal(1, await ScalarAsync(options, "SELECT COUNT(*) FROM audit_events;"));
        Assert.Equal(
            1,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM administration_leases;"));
    }

    [Fact]
    public async Task ResumedOperationUsesItsDurableStartForMutationIdentity()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        const string input = """
            {
              "targetId": "database",
              "expectedCurrentRevision": 0,
              "revision": 1,
              "categories": [
                { "id": "category", "displayName": "Category" }
              ],
              "databaseProducts": [
                {
                  "id": "database",
                  "revision": 1,
                  "displayName": "Database",
                  "status": "Candidate",
                  "categoryIds": ["category"]
                }
              ],
              "documentVersions": []
            }
            """;
        var inputPath = Path.Combine(root.InputRoot, "resumed.json");
        await File.WriteAllTextAsync(inputPath, input);
        var bytes = await File.ReadAllBytesAsync(inputPath);
        using var document = JsonDocument.Parse(bytes);
        var store = new SqliteControlPlaneStore(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var journal = new SqliteAdministrationCommandJournal(options);
        var corpusId = new CorpusId("admin-corpus");
        var operationId = new OperationId("resumed-operation");
        var actor = "os-sha256:" + new string('4', 64);
        const string reason = "Execute a bounded synthetic administration test.";
        var initialAudit = new AdministrativeAuditContext(
            operationId,
            actor,
            "add-database",
            reason,
            Now);
        var identifiers = executor.DescribeIntent(
            "add-database",
            corpusId,
            document.RootElement.Clone());
        var intent = new AdministrationJournalIntent(
            operationId,
            corpusId,
            "add-database",
            actor,
            initialAudit.ReasonSha256,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))
                .ToLowerInvariant(),
            identifiers.SourceIdentifiers,
            identifiers.TargetIdentifiers,
            Now);
        Assert.Equal(
            AdministrationJournalBeginOutcome.Started,
            (await journal.BeginAsync(intent)).Outcome);

        var result = await RunAsync(
            MutationArguments("add-database", operationId.Value, "resumed.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity(actor),
            new SqliteAdministrationLeaseManager(options),
            executor,
            journal,
            () => Now.AddHours(3));

        Assert.Equal(0, result.ExitCode);
        var expected = Now.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(
            expected,
            await TextScalarAsync(
                options,
                "SELECT occurred_at_utc FROM audit_events WHERE operation_id = 'resumed-operation';"));
        Assert.Equal(
            expected,
            await TextScalarAsync(
                options,
                "SELECT started_at_utc FROM administration_command_journal WHERE operation_id = 'resumed-operation';"));
    }

    [Fact]
    public async Task StrictPayloadRejectsUnknownFieldsAndReleasesTheLease()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "unknown.json"),
            """
            {
              "targetId": "database",
              "expectedCurrentRevision": 0,
              "revision": 1,
              "categories": [],
              "databaseProducts": [],
              "documentVersions": [],
              "unexpectedAuthority": true
            }
            """);

        var result = await RunAsync(
            MutationArguments("add-database", "operation-unknown", "unknown.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('e', 64)),
            new SqliteAdministrationLeaseManager(options),
            new SqliteAdministrativeCommandExecutor(new SqliteControlPlaneStore(options)),
            new SqliteAdministrationCommandJournal(options));

        Assert.Equal((int)AdministrationExitCode.InvalidInput, result.ExitCode);
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM administration_leases;"));
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM admin_operations;"));
        Assert.Equal(
            1,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed' AND exit_category = 2;"));
    }

    [Fact]
    public async Task MalformedPayloadIsDurablyRejectedAndDivergentReplayConflicts()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var path = Path.Combine(root.InputRoot, "malformed.json");
        await File.WriteAllTextAsync(path, "{\"targetId\":\"database\"");
        var arguments = MutationArguments(
            "add-database",
            "operation-malformed",
            "malformed.json");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('5', 64));
        var lease = new RecordingLeaseManager();
        var executor = new RecordingExecutor();
        var journal = new SqliteAdministrationCommandJournal(options);

        var first = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal);
        var replay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(5));
        await File.WriteAllTextAsync(path, "{\"targetId\":\"other\"");
        var divergentReplay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(10));

        Assert.Equal((int)AdministrationExitCode.InvalidInput, first.ExitCode);
        Assert.Equal((int)AdministrationExitCode.InvalidInput, replay.ExitCode);
        Assert.Equal((int)AdministrationExitCode.Conflict, divergentReplay.ExitCode);
        Assert.Empty(lease.Acquisitions);
        Assert.Empty(executor.Commands);
        Assert.Equal(
            1,
            await ScalarAsync(
                options,
                """
                SELECT COUNT(*)
                FROM administration_command_journal
                WHERE status = 'Completed'
                  AND exit_category = 2
                  AND input_sha256 IS NOT NULL;
                """));
    }

    [Fact]
    public async Task BoundedInputRejectsTraversalOversizeAndDuplicatePropertiesBeforeLease()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(
            Path.Combine(root.Root, "outside.json"),
            "{}");
        await File.WriteAllBytesAsync(
            Path.Combine(root.InputRoot, "oversize.json"),
            new byte[checked((int)OneShotAdministrationHost.MaximumInputBytes + 1)]);
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "duplicate.json"),
            "{\"targetId\":\"database\",\"targetId\":\"database\"}");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('8', 64));
        var lease = new RecordingLeaseManager();
        var executor = new RecordingExecutor();
        var journal = new RecordingJournal();
        var cases = new[]
        {
            (Operation: "reject-traversal", Path: $"..{Path.DirectorySeparatorChar}outside.json"),
            (Operation: "reject-oversize", Path: "oversize.json"),
            (Operation: "reject-duplicate", Path: "duplicate.json"),
        };

        foreach (var item in cases)
        {
            var result = await RunAsync(
                MutationArguments("add-database", item.Operation, item.Path),
                configuration,
                identity,
                lease,
                executor,
                journal);

            Assert.Equal((int)AdministrationExitCode.InvalidInput, result.ExitCode);
            Assert.Contains("CH_ADMIN_INPUT_REJECTED", result.Error, StringComparison.Ordinal);
        }

        Assert.Empty(lease.Acquisitions);
        Assert.Empty(executor.Commands);
    }

    [Fact]
    public async Task AuditWriteFailureIsDependencyUnavailableAndRollsBackTheMutation()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "audit-failure.json"),
            """
            {
              "targetId": "database",
              "expectedCurrentRevision": 0,
              "revision": 1,
              "categories": [
                { "id": "category", "displayName": "Category" }
              ],
              "databaseProducts": [
                {
                  "id": "database",
                  "revision": 1,
                  "displayName": "Database",
                  "status": "Candidate",
                  "categoryIds": ["category"]
                }
              ],
              "documentVersions": []
            }
            """);
        await ExecuteSqlAsync(
            options,
            """
            CREATE TRIGGER fail_admin_audit
            BEFORE INSERT ON audit_events
            BEGIN
                SELECT RAISE(ABORT, 'synthetic administrative audit failure');
            END;
            """);

        var arguments = MutationArguments(
            "add-database",
            "operation-audit-failure",
            "audit-failure.json");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('f', 64));
        var lease = new SqliteAdministrationLeaseManager(options);
        var executor = new SqliteAdministrativeCommandExecutor(
            new SqliteControlPlaneStore(options));
        var journal = new SqliteAdministrationCommandJournal(options);
        var result = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal);
        await ExecuteSqlAsync(options, "DROP TRIGGER fail_admin_audit;");
        var replay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(30));

        Assert.Equal((int)AdministrationExitCode.DependencyUnavailable, result.ExitCode);
        Assert.Equal((int)AdministrationExitCode.DependencyUnavailable, replay.ExitCode);
        Assert.Contains(
            "CH_ADMIN_DEPENDENCY_UNAVAILABLE",
            result.Error,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "synthetic administrative audit failure",
            result.Error,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM catalogue_heads;"));
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM admin_operations;"));
        Assert.Equal(0, await ScalarAsync(options, "SELECT COUNT(*) FROM administration_leases;"));
        Assert.Equal(
            1,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed' AND exit_category = 5;"));
    }

    [Fact]
    public async Task OperationalFailuresUseCanonicalPhaseExitCategories()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "input.json"), "{}");
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('a', 64));

        var missingInput = await RunAsync(
            MutationArguments("add-database", "missing-input", "missing.json"),
            configuration,
            identity,
            new RecordingLeaseManager(),
            new RecordingExecutor(),
            new RecordingJournal());
        AssertCanonicalFailure(
            missingInput,
            AdministrationExitCode.InvalidInput,
            "CH_ADMIN_INPUT_REJECTED");

        var journalUnavailable = await RunAsync(
            MutationArguments("add-database", "journal-unavailable", "input.json"),
            configuration,
            identity,
            new RecordingLeaseManager(),
            new RecordingExecutor(),
            new RecordingJournal
            {
                BeginFailure = new IOException("Sensitive journal detail."),
            });
        AssertCanonicalFailure(
            journalUnavailable,
            AdministrationExitCode.DependencyUnavailable,
            "CH_ADMIN_DEPENDENCY_UNAVAILABLE");

        var leaseUnavailable = await RunAsync(
            MutationArguments("add-database", "lease-unavailable", "input.json"),
            configuration,
            identity,
            new RecordingLeaseManager
            {
                AcquireFailure = new IOException("Sensitive lease detail."),
            },
            new RecordingExecutor(),
            new RecordingJournal());
        AssertCanonicalFailure(
            leaseUnavailable,
            AdministrationExitCode.DependencyUnavailable,
            "CH_ADMIN_DEPENDENCY_UNAVAILABLE");

        var executorUnavailable = await RunAsync(
            MutationArguments("add-database", "executor-unavailable", "input.json"),
            configuration,
            identity,
            new RecordingLeaseManager(),
            new RecordingExecutor
            {
                ExecuteFailure = new IOException("Sensitive executor detail."),
            },
            new RecordingJournal());
        AssertCanonicalFailure(
            executorUnavailable,
            AdministrationExitCode.DependencyUnavailable,
            "CH_ADMIN_DEPENDENCY_UNAVAILABLE");

        var operationConflict = await RunAsync(
            MutationArguments("add-database", "operation-conflict", "input.json"),
            configuration,
            identity,
            new RecordingLeaseManager(),
            new RecordingExecutor
            {
                ExecuteFailure = new InvalidOperationException("Sensitive conflict detail."),
            },
            new RecordingJournal());
        AssertCanonicalFailure(
            operationConflict,
            AdministrationExitCode.Conflict,
            "CH_ADMIN_OPERATION_CONFLICT");
    }

    [Fact]
    public async Task RealCatalogueCommandsEnforceExactLifecycleAndRejectCollateralChanges()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var lease = new SqliteAdministrationLeaseManager(options);
        var journal = new SqliteAdministrationCommandJournal(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('1', 64));
        var categories = new[] { new CategoryPlan("category", "Category") };

        async Task<RunResult> ExecuteAsync(
            string command,
            string operationId,
            CataloguePlan plan)
        {
            var fileName = $"{operationId}.json";
            await File.WriteAllTextAsync(
                Path.Combine(root.InputRoot, fileName),
                JsonSerializer.Serialize(plan, PlanJsonOptions));
            return await RunAsync(
                MutationArguments(command, operationId, fileName),
                configuration,
                identity,
                lease,
                executor,
                journal);
        }

        DatabaseProductPlan Product(
            long revision,
            string status,
            string displayName = "Database") =>
            new("database", revision, displayName, status, ["category"]);
        DocumentPlan Document(
            string id,
            long version,
            long productRevision,
            string status,
            char digestCharacter) =>
            new(
                id,
                version,
                "database",
                productRevision,
                "Pdf",
                "en-GB",
                status,
                new string(digestCharacter, 64),
                128,
                "application/pdf",
                "pdfpig",
                "LocalAuthorised",
                null,
                null);

        var documentOneCandidate = Document(
            "document-one",
            1,
            1,
            "Candidate",
            'a');
        var documentOneActive = documentOneCandidate with { Status = "Active" };
        var documentOneDeactivated = documentOneCandidate with { Status = "Deactivated" };
        var documentTwoCandidate = Document(
            "document-two",
            1,
            1,
            "Candidate",
            'b');
        var documentTwoActive = documentTwoCandidate with { Status = "Active" };
        var documentTwoDeactivated = documentTwoCandidate with { Status = "Deactivated" };

        Assert.Equal(0, (await ExecuteAsync(
            "add-database",
            "lifecycle-01-add-database",
            new("database", null, 0, 1, categories, [Product(1, "Candidate")], [])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "add-document",
            "lifecycle-02-add-document-one",
            new(
                "document-one",
                1,
                1,
                2,
                categories,
                [Product(1, "Candidate")],
                [documentOneCandidate])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "activate-database",
            "lifecycle-03-activate-database",
            new(
                "database",
                null,
                2,
                3,
                categories,
                [Product(1, "Active")],
                [documentOneActive])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "add-document",
            "lifecycle-04-add-document-two",
            new(
                "document-two",
                1,
                3,
                4,
                categories,
                [Product(1, "Active")],
                [documentOneActive, documentTwoCandidate])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "activate-document",
            "lifecycle-05-activate-document-two",
            new(
                "document-two",
                1,
                4,
                5,
                categories,
                [Product(1, "Active")],
                [documentOneActive, documentTwoActive])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "deactivate-document",
            "lifecycle-06-deactivate-document-one",
            new(
                "document-one",
                1,
                5,
                6,
                categories,
                [Product(1, "Active")],
                [documentOneDeactivated, documentTwoActive])))
            .ExitCode);

        var lastDocumentRejection = await ExecuteAsync(
            "deactivate-document",
            "lifecycle-07-reject-last-document",
            new(
                "document-two",
                1,
                6,
                7,
                categories,
                [Product(1, "Active")],
                [documentOneDeactivated, documentTwoDeactivated]));
        Assert.Equal((int)AdministrationExitCode.InvalidInput, lastDocumentRejection.ExitCode);

        Assert.Equal(0, (await ExecuteAsync(
            "deactivate-database",
            "lifecycle-08-deactivate-database",
            new(
                "database",
                null,
                6,
                7,
                categories,
                [Product(1, "Deactivated")],
                [documentOneDeactivated, documentTwoDeactivated])))
            .ExitCode);
        var reboundDocumentOne = documentOneDeactivated with { DatabaseProductRevision = 2 };
        var reboundDocumentTwo = documentTwoDeactivated with { DatabaseProductRevision = 2 };
        Assert.Equal(0, (await ExecuteAsync(
            "version-database",
            "lifecycle-09-version-database",
            new(
                "database",
                null,
                7,
                8,
                categories,
                [Product(2, "Candidate")],
                [reboundDocumentOne, reboundDocumentTwo])))
            .ExitCode);

        var invalidActiveVersion = Document(
            "document-one",
            2,
            2,
            "Active",
            'c');
        var activeVersionRejection = await ExecuteAsync(
            "version-document",
            "lifecycle-10-reject-active-version",
            new(
                "document-one",
                2,
                8,
                9,
                categories,
                [Product(2, "Candidate")],
                [reboundDocumentOne, invalidActiveVersion, reboundDocumentTwo]));
        Assert.Equal((int)AdministrationExitCode.InvalidInput, activeVersionRejection.ExitCode);

        var documentOneVersionTwo = invalidActiveVersion with { Status = "Candidate" };
        Assert.Equal(0, (await ExecuteAsync(
            "version-document",
            "lifecycle-11-version-document",
            new(
                "document-one",
                2,
                8,
                9,
                categories,
                [Product(2, "Candidate")],
                [reboundDocumentOne, documentOneVersionTwo, reboundDocumentTwo])))
            .ExitCode);

        var collateralRejection = await ExecuteAsync(
            "version-document",
            "lifecycle-12-reject-collateral",
            new(
                "document-two",
                2,
                9,
                10,
                categories,
                [Product(2, "Candidate", "Changed without authority")],
                [
                    reboundDocumentOne,
                    documentOneVersionTwo,
                    reboundDocumentTwo,
                    Document("document-two", 2, 2, "Candidate", 'd'),
                ]));
        Assert.Equal((int)AdministrationExitCode.InvalidInput, collateralRejection.ExitCode);

        var current = await store.ReadCurrentCatalogueAsync(new CorpusId("admin-corpus"));
        Assert.NotNull(current);
        Assert.Equal(9, current.Revision.Value);
        var product = Assert.Single(current.DatabaseProducts);
        Assert.Equal(2, product.Revision.Value);
        Assert.Equal(CatalogueItemStatus.Candidate, product.Status);
        Assert.Equal(3, current.DocumentVersions.Count);
        Assert.Equal(
            12,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));
        Assert.Equal(9, await ScalarAsync(options, "SELECT COUNT(*) FROM admin_operations;"));
    }

    [Fact]
    public async Task DocumentVersionReplacementIsAtomicAtFifthCatalogueRevision()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var lease = new SqliteAdministrationLeaseManager(options);
        var journal = new SqliteAdministrationCommandJournal(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('1', 64));
        var categories = new[] { new CategoryPlan("category", "Category") };

        async Task<RunResult> ExecuteAsync(
            string command,
            string operationId,
            CataloguePlan plan)
        {
            var fileName = $"{operationId}.json";
            await File.WriteAllTextAsync(
                Path.Combine(root.InputRoot, fileName),
                JsonSerializer.Serialize(plan, PlanJsonOptions));
            return await RunAsync(
                MutationArguments(command, operationId, fileName),
                configuration,
                identity,
                lease,
                executor,
                journal);
        }

        DatabaseProductPlan Product(string status) =>
            new("database", 1, "Database", status, ["category"]);
        DocumentPlan Document(long version, string status, char digestCharacter) =>
            new(
                "document",
                version,
                "database",
                1,
                "Pdf",
                "en-GB",
                status,
                new string(digestCharacter, 64),
                128,
                "application/pdf",
                version == 1 ? "local-pdf" : "official-pdf",
                version == 1 ? "LocalAuthorised" : "OfficialExternal",
                version == 1 ? null : "official-registration",
                version == 1 ? null : $"snapshot-{new string('c', 64)}");

        var versionOneCandidate = Document(1, "Candidate", 'a');
        var versionOneActive = versionOneCandidate with { Status = "Active" };
        var versionOneDeactivated = versionOneCandidate with { Status = "Deactivated" };
        var versionTwoCandidate = Document(2, "Candidate", 'b');
        var versionTwoActive = versionTwoCandidate with { Status = "Active" };

        Assert.Equal(0, (await ExecuteAsync(
            "add-database",
            "replacement-01-add-database",
            new("database", null, 0, 1, categories, [Product("Candidate")], [])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "add-document",
            "replacement-02-add-document",
            new(
                "document",
                1,
                1,
                2,
                categories,
                [Product("Candidate")],
                [versionOneCandidate])))
            .ExitCode);
        Assert.Equal(0, (await ExecuteAsync(
            "activate-database",
            "replacement-03-activate-database",
            new(
                "database",
                null,
                2,
                3,
                categories,
                [Product("Active")],
                [versionOneActive])))
            .ExitCode);
        var officialRegistration = new OfficialSourceRegistration(
            new OfficialSourceRegistrationId("official-registration"),
            new SourceRegistrationRevision(1),
            new DatabaseProductId("database"),
            new DocumentId("document"),
            new SourceAdapterId("official-pdf"),
            "https://example.invalid/document.pdf",
            CatalogueItemStatus.Candidate);
        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await store.RegisterOfficialSourceAsync(
                new OfficialSourceRegistrationCommitRequest(
                    new OperationId("replacement-official-registration"),
                    new CorpusId("admin-corpus"),
                    officialRegistration,
                    Now))).Outcome);
        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await store.CommitOfficialSourceAsync(
                new OfficialSourceCommitRequest(
                    new OperationId("replacement-official-snapshot"),
                    new CorpusId("admin-corpus"),
                    officialRegistration,
                    new OfficialSourceSnapshot(
                        new OfficialSnapshotId($"snapshot-{new string('c', 64)}"),
                        officialRegistration.Id,
                        new ContentObjectId(new string('b', 64)),
                        128,
                        "application/pdf",
                        Now),
                    Now))).Outcome);
        Assert.Equal(0, (await ExecuteAsync(
            "version-document",
            "replacement-04-version-document",
            new(
                "document",
                2,
                3,
                4,
                categories,
                [Product("Active")],
                [versionOneActive, versionTwoCandidate])))
            .ExitCode);
        var collateralReplacement = versionOneDeactivated with { ByteLength = 129 };
        var collateralRejection = await ExecuteAsync(
            "activate-document",
            "replacement-05-reject-collateral",
            new(
                "document",
                2,
                4,
                5,
                categories,
                [Product("Active")],
                [collateralReplacement, versionTwoActive]));
        Assert.Equal((int)AdministrationExitCode.InvalidInput, collateralRejection.ExitCode);

        Assert.Equal(0, (await ExecuteAsync(
            "activate-document",
            "replacement-05-activate-official",
            new(
                "document",
                2,
                4,
                5,
                categories,
                [Product("Active")],
                [versionOneDeactivated, versionTwoActive])))
            .ExitCode);

        var current = await store.ReadCurrentCatalogueAsync(new CorpusId("admin-corpus"));
        Assert.NotNull(current);
        Assert.Equal(5, current.Revision.Value);
        Assert.Equal(CatalogueItemStatus.Active, Assert.Single(current.DatabaseProducts).Status);
        Assert.Collection(
            current.DocumentVersions.OrderBy(document => document.Version.Value),
            document => Assert.Equal(CatalogueItemStatus.Deactivated, document.Status),
            document => Assert.Equal(CatalogueItemStatus.Active, document.Status));
    }

    [Fact]
    public async Task OfficialRegistrationReplayIsExactAndAttemptTimeIndependent()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var corpusId = new CorpusId("admin-corpus");
        var snapshot = new CatalogueSnapshot(
            corpusId,
            new CatalogueRevision(1),
            [new DatabaseCategory(new DatabaseCategoryId("category"), "Category")],
            [new DatabaseProduct(
                new DatabaseProductId("database"),
                new DatabaseProductRevision(1),
                "Database",
                CatalogueItemStatus.Candidate,
                [new DatabaseCategoryId("category")])],
            [new DocumentVersion(
                new DocumentId("document"),
                new DocumentVersionNumber(1),
                new DatabaseProductId("database"),
                new DatabaseProductRevision(1),
                DocumentFormat.Csv,
                DocumentContentLanguage.EnGb,
                CatalogueItemStatus.Candidate,
                new ContentObjectId(new string('e', 64)),
                128,
                "text/csv",
                new SourceAdapterId("csvhelper"),
                SourceTrustClass.LocalAuthorised)]);
        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await store.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("registration-prerequisite"),
                snapshot,
                0,
                Now))).Outcome);
        const string registration = """
            {
              "registrationId": "official-registration",
              "revision": 1,
              "databaseProductId": "database",
              "documentId": "document",
              "sourceAdapterId": "official-exact-http",
              "canonicalHttpsUrl": "https://official.invalid/document.csv",
              "status": "Candidate"
            }
            """;
        const string divergentRegistration = """
            {
              "registrationId": "official-registration",
              "revision": 1,
              "databaseProductId": "database",
              "documentId": "document",
              "sourceAdapterId": "official-exact-http",
              "canonicalHttpsUrl": "https://official.invalid/other.csv",
              "status": "Candidate"
            }
            """;
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "registration.json"),
            registration);
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "registration-divergent.json"),
            divergentRegistration);
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('2', 64));
        var lease = new SqliteAdministrationLeaseManager(options);
        var journal = new SqliteAdministrationCommandJournal(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var arguments = MutationArguments(
            "register-official-source",
            "registration-operation",
            "registration.json");

        var first = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddMinutes(1));
        var replay = await RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddHours(2));
        var divergent = await RunAsync(
            MutationArguments(
                "register-official-source",
                "registration-operation",
                "registration-divergent.json"),
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddHours(3));

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(0, replay.ExitCode);
        Assert.Equal((int)AdministrationExitCode.Conflict, divergent.ExitCode);
        Assert.Contains("CH_ADMIN_APPLIED", replay.Output, StringComparison.Ordinal);
        Assert.Contains("CH_ADMIN_OPERATION_CONFLICT", divergent.Error, StringComparison.Ordinal);
        Assert.Equal(
            1,
            await ScalarAsync(options, "SELECT COUNT(*) FROM official_source_registrations;"));
        Assert.Equal(
            1,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE operation_id = 'registration-operation' AND status = 'Completed' AND exit_category = 0;"));
    }

    [Fact]
    public async Task CatalogueCommandsRejectDocumentOwnershipTransferAndUnusedCategories()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var categories = new[]
        {
            new CategoryPlan("category-one", "Category one"),
            new CategoryPlan("category-two", "Category two"),
        };
        var productOne = new DatabaseProductPlan(
            "database-one",
            1,
            "Database one",
            "Candidate",
            ["category-one"]);
        var productTwo = new DatabaseProductPlan(
            "database-two",
            1,
            "Database two",
            "Candidate",
            ["category-two"]);
        var documentOne = new DocumentPlan(
            "document-one",
            1,
            "database-one",
            1,
            "Csv",
            "en-GB",
            "Candidate",
            new string('7', 64),
            64,
            "text/csv",
            "csvhelper",
            "LocalAuthorised",
            null,
            null);
        var prerequisite = new CatalogueSnapshot(
            new CorpusId("admin-corpus"),
            new CatalogueRevision(1),
            categories.Select(category => new DatabaseCategory(
                new DatabaseCategoryId(category.Id),
                category.DisplayName)),
            new[] { productOne, productTwo }.Select(product => new DatabaseProduct(
                new DatabaseProductId(product.Id),
                new DatabaseProductRevision(product.Revision),
                product.DisplayName,
                CatalogueItemStatus.Candidate,
                product.CategoryIds.Select(id => new DatabaseCategoryId(id)))),
            [new DocumentVersion(
                new DocumentId(documentOne.Id),
                new DocumentVersionNumber(documentOne.Version),
                new DatabaseProductId(documentOne.DatabaseProductId),
                new DatabaseProductRevision(documentOne.DatabaseProductRevision),
                DocumentFormat.Csv,
                DocumentContentLanguage.EnGb,
                CatalogueItemStatus.Candidate,
                new ContentObjectId(documentOne.ContentObjectId),
                documentOne.ByteLength,
                documentOne.MediaType,
                new SourceAdapterId(documentOne.SourceAdapterId),
                SourceTrustClass.LocalAuthorised)]);
        Assert.Equal(
            StoreMutationOutcome.Applied,
            (await store.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("ownership-prerequisite"),
                prerequisite,
                0,
                Now))).Outcome);
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);
        var identity = new StubIdentity("os-sha256:" + new string('6', 64));
        var lease = new SqliteAdministrationLeaseManager(options);
        var journal = new SqliteAdministrationCommandJournal(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);

        var movedVersion = documentOne with
        {
            Version = 2,
            DatabaseProductId = "database-two",
            ContentObjectId = new string('8', 64),
        };
        var transfer = await ExecutePlanAsync(
            "version-document",
            "reject-document-transfer",
            new CataloguePlan(
                "document-one",
                2,
                1,
                2,
                categories,
                [productOne, productTwo],
                [documentOne, movedVersion]));
        var productOneRevisionTwo = productOne with
        {
            Revision = 2,
            CategoryIds = ["category-two"],
        };
        var reboundDocument = documentOne with { DatabaseProductRevision = 2 };
        var unusedCategory = await ExecutePlanAsync(
            "version-database",
            "reject-unused-category",
            new CataloguePlan(
                "database-one",
                null,
                1,
                2,
                categories,
                [productOneRevisionTwo, productTwo],
                [reboundDocument]));

        Assert.Equal((int)AdministrationExitCode.InvalidInput, transfer.ExitCode);
        Assert.Equal((int)AdministrationExitCode.InvalidInput, unusedCategory.ExitCode);
        Assert.Equal(1, (await store.ReadCurrentCatalogueAsync(
            new CorpusId("admin-corpus")))!.Revision.Value);
        Assert.Equal(
            2,
            await ScalarAsync(
                options,
                """
                SELECT COUNT(*)
                FROM administration_command_journal
                WHERE status = 'Completed' AND exit_category = 2;
                """));

        async Task<RunResult> ExecutePlanAsync(
            string command,
            string operationId,
            CataloguePlan plan)
        {
            var fileName = $"{operationId}.json";
            await File.WriteAllTextAsync(
                Path.Combine(root.InputRoot, fileName),
                JsonSerializer.Serialize(plan, PlanJsonOptions));
            return await RunAsync(
                MutationArguments(command, operationId, fileName),
                configuration,
                identity,
                lease,
                executor,
                journal);
        }
    }

    [Fact]
    public async Task StatusAndUnavailableResultsAreDurableExactReplays()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "build.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "sync.json"), "{}");
        var store = new SqliteControlPlaneStore(options);
        var journal = new SqliteAdministrationCommandJournal(options);
        var executor = new SqliteAdministrativeCommandExecutor(store);
        var lease = new RecordingLeaseManager();
        var identity = new StubIdentity("os-sha256:" + new string('3', 64));
        var configuration = Configuration(true, root.InputRoot, root.StoreRoot);

        var status = await RunAsync(
            StatusArguments("status-operation"),
            configuration,
            identity,
            lease,
            executor,
            journal);
        var statusReplay = await RunAsync(
            StatusArguments("status-operation"),
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddHours(1));
        var buildArguments = MutationArguments(
            "build-index",
            "build-unavailable-operation",
            "build.json");
        var unavailable = await RunAsync(
            buildArguments,
            configuration,
            identity,
            lease,
            executor,
            journal);
        var unavailableReplay = await RunAsync(
            buildArguments,
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddHours(2));
        var syncUnavailable = await RunAsync(
            MutationArguments(
                "synchronise-official",
                "sync-unavailable-operation",
                "sync.json"),
            configuration,
            identity,
            lease,
            executor,
            journal,
            () => Now.AddHours(3));

        Assert.Equal(0, status.ExitCode);
        Assert.Equal(0, statusReplay.ExitCode);
        Assert.Contains("CH_ADMIN_STATUS_EMPTY", statusReplay.Output, StringComparison.Ordinal);
        Assert.Equal((int)AdministrationExitCode.DependencyUnavailable, unavailable.ExitCode);
        Assert.Equal(
            (int)AdministrationExitCode.DependencyUnavailable,
            unavailableReplay.ExitCode);
        Assert.Contains(
            "CH_ADMIN_CAPABILITY_NOT_COMPOSED",
            unavailableReplay.Error,
            StringComparison.Ordinal);
        Assert.Equal(
            (int)AdministrationExitCode.DependencyUnavailable,
            syncUnavailable.ExitCode);
        Assert.Contains(
            "CH_ADMIN_CAPABILITY_NOT_COMPOSED",
            syncUnavailable.Error,
            StringComparison.Ordinal);
        Assert.Equal(2, lease.Acquisitions.Count);
        Assert.Equal(2, lease.Releases.Count);
        Assert.Equal(
            3,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));
        Assert.Equal(
            2,
            await ScalarAsync(
                options,
                "SELECT COUNT(*) FROM administration_command_journal WHERE outcome = 'Unavailable' AND exit_category = 5;"));
    }

    [Fact]
    public void ProductionCompositionRejectsIncompleteMaterialisationPortPairs()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        var store = new SqliteControlPlaneStore(options);
        var descriptor = new EmbeddingProviderDescriptor(
            "synthetic-provider",
            "synthetic-model",
            "synthetic-revision-1",
            dimensions: 2);
        var embedding = new CountingEmbeddingProvider(descriptor);
        Assert.Throws<ArgumentException>(() =>
            AdministrativeMaterialisationComposition.CreateExecutor(
                options,
                store,
                new AdministrativeMaterialisationPorts(EmbeddingProvider: embedding)));
        Assert.Throws<ArgumentException>(() =>
            AdministrativeMaterialisationComposition.CreateExecutor(
                options,
                store,
                new AdministrativeMaterialisationPorts(
                    OfficialSourceTransport: new CountingOfficialSourceTransport(
                        new OfficialFetchResult(
                            OfficialFetchStatus.Changed,
                            statusCode: 200,
                            SyntheticAdministrativeMaterialisationProfile.SourceBytes,
                            ContentMediaType.TextCsv.Value,
                            "\"synthetic-v1\"",
                            Now)))));
        Assert.Throws<ArgumentException>(() =>
            AdministrativeMaterialisationComposition.CreateExecutor(
                options,
                store,
                new AdministrativeMaterialisationPorts(
                    RenderManifestStore: store)));
        Assert.Equal(0, embedding.CallCount);
    }

    [Fact]
    public async Task ProgramPathLeavesMaterialisationUnavailableWithoutExplicitProfile()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        await File.WriteAllTextAsync(Path.Combine(root.InputRoot, "build.json"), "{}");

        var result = await RunProgramAsync(
            MutationArguments(
                "build-index",
                "build-program-unavailable-operation",
                "build.json"),
            root,
            materialisationProfile: null);

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.DependencyUnavailable,
            "CH_ADMIN_CAPABILITY_NOT_COMPOSED");
    }

    [Fact]
    public async Task ProgramPathRejectsSyntheticProfileOutsideIntegrationEnvironment()
    {
        using var root = TemporaryAdministrationRoot.Create();

        var result = await RunProgramAsync(
            StatusArguments("profile-environment-mismatch-operation"),
            root,
            environment: "Production");

        AssertCanonicalFailure(
            result,
            AdministrationExitCode.ConfigurationOrAuthorityDenied,
            "CH_ADMIN_CONFIGURATION_INVALID");
    }

    [Fact]
    public async Task ProgramPathReachesExplicitProductProfileWithoutReadingCredentialOrExternalAccess()
    {
        using var root = TemporaryAdministrationRoot.Create();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(root.CreateStoreOptions());

        var result = await RunProgramAsync(
            StatusArguments("product-profile-status-operation"),
            root,
            ProductAdministrativeMaterialisationProfile.ProfileName,
            environment: "Production",
            enableProductProfile: true);

        Assert.Equal((int)AdministrationExitCode.Success, result.ExitCode);
        Assert.Contains("CH_ADMIN_STATUS_EMPTY", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("CH_ADMIN_CONFIGURATION_INVALID", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductProfileIsTypedFrozenAndLeavesCredentialReferenceLazy()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var credentialReads = 0;
        var authority = new NullOfficialSourceAuthorityResolver();
        var transport = new CountingOfficialSourceTransport(new OfficialFetchResult(
            OfficialFetchStatus.NotModified,
            statusCode: 304,
            content: null,
            mediaType: null,
            etag: null,
            lastModified: null));
        var handler = new RejectingHttpMessageHandler();
        var dependencies = new ProductAdministrativeMaterialisationDependencies(
            _ => authority,
            () => transport,
            () => new HttpClient(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(25),
            },
            _ =>
            {
                credentialReads++;
                throw new InvalidOperationException("A credential must stay lazy.");
            },
            options => new SqliteControlPlaneStore(options),
            () => new IsolatedPdfRendererProcess(new RendererWorkerLaunch(
                "synthetic-renderer.exe",
                [],
                "win-x64")),
            () => new PngPageImageValidator(),
            () => new NoticeBearingPageImageCompositor());
        var ports = ProductAdministrativeMaterialisationProfile.Resolve(
            ProductProfileConfiguration(),
            root.CreateStoreOptions(),
            dependencies);

        Assert.Same(authority, ports.OfficialSourceAuthorityResolver);
        Assert.Same(transport, ports.OfficialSourceTransport);
        Assert.NotNull(ports.EmbeddingProvider);
        Assert.NotNull(ports.RenderManifestStore);
        Assert.NotNull(ports.PdfPageRenderer);
        Assert.NotNull(ports.PngPageImageValidator);
        Assert.NotNull(ports.NoticeBearingCompositor);
        Assert.Same(ports.NoticeBearingCompositor, ports.NoticeBearingValidator);
        Assert.Same(
            ProductAdministrativeMaterialisationProfile.CompatibilityProfile,
            ports.IndexCompatibilityProfile);
        Assert.Equal(
            ProductAdministrativeMaterialisationProfile.ExpectedCompatibilityKey,
            ports.IndexCompatibilityProfile!.Key.Value);
        Assert.Equal(0, credentialReads);
        Assert.Equal(0, transport.CallCount);
        Assert.Equal(0, handler.CallCount);
        Assert.Equal(
            [
                CsvHelperDocumentParser.CompatibilityDescriptor,
                PdfPigDocumentParser.CompatibilityDescriptor,
            ],
            ports.IndexCompatibilityProfile.ParserDescriptors);
        Assert.Equal(
            ChunkingPolicy.DefaultTargetScalarCount,
            ports.IndexCompatibilityProfile.ChunkingPolicy.TargetScalarCount);
        Assert.Equal(
            ProductAdministrativeMaterialisationProfile.EmbeddingDescriptor,
            ports.IndexCompatibilityProfile.EmbeddingDescriptor);
        Assert.Equal(
            SqliteVectorIndexStore.CompatibilityDescriptor,
            ports.IndexCompatibilityProfile.VectorStoreDescriptor);
    }

    [Theory]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Enabled", "false")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:OfficialSource:Enabled", "false")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Rendering:Enabled", "false")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Rendering:ProfileId", "pdf-page-png-v1")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Embedding:Dimensions", "3072")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Embedding:ModelRevision", "drifted")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Embedding:CredentialEnvironmentVariable", "invalid-secret-reference")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Embedding:OperationalAuthorityReference", "invalid-authority")]
    [InlineData("RagChallenge:Administration:ProductMaterialisation:Embedding:TrustedOperationalGrantReference", "invalid-authority")]
    public void ProductProfileRejectsDisabledIncompleteOrDriftedConfiguration(
        string key,
        string value)
    {
        using var root = TemporaryAdministrationRoot.Create();
        var values = ProductProfileValues();
        values[key] = value;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        Assert.Throws<ArgumentException>(() =>
            ProductAdministrativeMaterialisationProfile.Resolve(
                configuration,
                root.CreateStoreOptions()));
    }

    [Fact]
    public async Task ComposedBuildIndexUsesGovernedSyntheticDependenciesAndRejectsProfileDrift()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var vectorStore = new SqliteVectorIndexStore(options);
        var contentStore = new ImmutableContentStore(options);
        var corpusId = new CorpusId("admin-corpus");
        var productId = new DatabaseProductId("admin-database");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("admin-build-document");
        var documentVersion = new DocumentVersionNumber(1);
        var sourceAdapterId = new SourceAdapterId("synthetic-local-csv");
        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(
            "feature,description\nindex,deterministic synthetic materialisation\n");
        await using var source = new MemoryStream(sourceBytes, writable: false);
        var content = await contentStore.PutAndVerifyAsync(new BoundedContentInput(
            source,
            sourceBytes.Length,
            ContentMediaType.TextCsv));
        var category = new DatabaseCategory(
            new DatabaseCategoryId("admin-category"),
            "Administration category");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Administration database",
            CatalogueItemStatus.Active,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Active,
            content.ContentObjectId,
            content.ByteLength,
            ContentMediaType.TextCsv.Value,
            sourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await store.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("admin-build-catalogue"),
                new CatalogueSnapshot(corpusId, new CatalogueRevision(1),
                    [category], [product], [document]),
                ExpectedCurrentRevision: 0,
                Now))).Outcome);
        var binding = new DocumentBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            sourceAdapterId,
            SourceTrustClass.LocalAuthorised);
        var compatibility =
            SyntheticAdministrativeMaterialisationProfile.CompatibilityProfile;
        var rights = CreateTextualRightsPlan(documentId, documentVersion);
        var activeDigest = BindingDigestCanonicalizer
            .CanonicaliseActiveDocumentSet([binding]).Digest.Value;
        var sourceDigest = BindingDigestCanonicalizer
            .CanonicaliseSourceBindingSet([binding]).Digest.Value;

        await WriteBuildPlanAsync(
            "build-composed.json",
            "candidate-admin-composed",
            compatibility.Key.Value);
        var applied = await RunProgramAsync(
            MutationArguments(
                "build-index",
                "build-composed-operation",
                "build-composed.json"),
            root);
        var replay = await RunProgramAsync(
            MutationArguments(
                "build-index",
                "build-composed-operation",
                "build-composed.json"),
            root);
        await WriteBuildPlanAsync(
            "build-divergent.json",
            "candidate-admin-divergent",
            new string('f', 64));
        var rejected = await RunProgramAsync(
            MutationArguments(
                "build-index",
                "build-divergent-operation",
                "build-divergent.json"),
            root);

        Assert.True(
            applied.ExitCode == (int)AdministrationExitCode.Success,
            applied.Output + applied.Error);
        Assert.Contains("CH_ADMIN_APPLIED", applied.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "CH_ADMIN_CAPABILITY_NOT_COMPOSED",
            applied.Output + applied.Error,
            StringComparison.Ordinal);
        Assert.Equal((int)AdministrationExitCode.Success, replay.ExitCode);
        Assert.Contains("AlreadyApplied", replay.Output, StringComparison.Ordinal);
        Assert.Equal((int)AdministrationExitCode.Conflict, rejected.ExitCode);
        Assert.Contains(
            "CH_ADMIN_VALIDATION_FAILED",
            rejected.Error,
            StringComparison.Ordinal);
        Assert.Equal(1, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM generation_manifests;"));
        Assert.Equal(2, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));

        async Task WriteBuildPlanAsync(
            string fileName,
            string candidateBuildId,
            string expectedCompatibilityKey)
        {
            var plan = new
            {
                candidateBuildId,
                corpusRevision = 1,
                catalogueRevision = 1,
                activeDocumentSetDigest = activeDigest,
                sourceBindingSetDigest = sourceDigest,
                expectedIndexCompatibilityKey = expectedCompatibilityKey,
                maximumEmbeddingBatchUtf8Bytes = 16_384,
                documents = new[]
                {
                    new
                    {
                        binding = new
                        {
                            databaseProductId = productId.Value,
                            databaseProductRevision = productRevision.Value,
                            documentId = documentId.Value,
                            documentVersion = documentVersion.Value,
                            documentFormat = DocumentFormat.Csv.ToString(),
                            sourceAdapterId = sourceAdapterId.Value,
                            sourceTrustClass = SourceTrustClass.LocalAuthorised.ToString(),
                            officialSourceRegistrationId = (string?)null,
                            officialSnapshotId = (string?)null,
                            sourceObservationId = (string?)null,
                        },
                        contentLanguage = DocumentContentLanguage.EnGb.ToCanonicalTag(),
                        sourceContentObjectId = content.ContentObjectId.Value,
                        byteLength = content.ByteLength,
                        mediaType = content.MediaType.Value,
                        parserPolicy = new
                        {
                            maximumByteLength = 4096,
                            maximumUnits = 32,
                            maximumTextCharacters = 4096,
                            maximumFieldsPerRecord = 16,
                            maximumFieldCharacters = 1024,
                        },
                        rights,
                    },
                },
            };
            await File.WriteAllTextAsync(
                Path.Combine(root.InputRoot, fileName),
                JsonSerializer.Serialize(plan, PlanJsonOptions));
        }
    }

    [Fact]
    public async Task ComposedOfficialSynchronisationUsesBoundLeaseAndStopsBeforeTransportOnDrift()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var store = new SqliteControlPlaneStore(options);
        var contentStore = new ImmutableContentStore(options);
        var corpusId = new CorpusId("admin-corpus");
        var productId = new DatabaseProductId("admin-database");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("admin-official-document");
        var documentVersion = new DocumentVersionNumber(1);
        var registrationId = new OfficialSourceRegistrationId(
            "synthetic-official-registration");
        var sourceAdapterId = new SourceAdapterId("synthetic-official-csv");
        var stagingAdapterId = new SourceAdapterId("synthetic-local-staging");
        var sourceBytes = SyntheticAdministrativeMaterialisationProfile.SourceBytes;
        await using var stagedSource = new MemoryStream(sourceBytes, writable: false);
        var staged = await contentStore.PutAndVerifyAsync(new BoundedContentInput(
            stagedSource,
            sourceBytes.Length,
            ContentMediaType.TextCsv));
        var category = new DatabaseCategory(
            new DatabaseCategoryId("admin-category"),
            "Administration category");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "Administration database",
            CatalogueItemStatus.Candidate,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            CatalogueItemStatus.Candidate,
            staged.ContentObjectId,
            staged.ByteLength,
            staged.MediaType.Value,
            stagingAdapterId,
            SourceTrustClass.LocalAuthorised);
        Assert.Equal(StoreMutationOutcome.Applied, (
            await store.CommitCatalogueAsync(new CatalogueCommitRequest(
                new OperationId("admin-sync-catalogue"),
                new CatalogueSnapshot(corpusId, new CatalogueRevision(1),
                    [category], [product], [document]),
                ExpectedCurrentRevision: 0,
                Now))).Outcome);
        var registration =
            SyntheticAdministrativeMaterialisationProfile.Registration;
        Assert.Equal(StoreMutationOutcome.Applied, (
            await store.RegisterOfficialSourceAsync(
                new OfficialSourceRegistrationCommitRequest(
                    new OperationId("admin-sync-registration"),
                    corpusId,
                    registration,
                    Now))).Outcome);
        var rights = CreateTextualRightsPlan(documentId, documentVersion);

        await WriteSyncPlanAsync("sync-composed.json", registrationRevision: 1);
        var applied = await RunProgramAsync(
            MutationArguments(
                "synchronise-official",
                "sync-composed-operation",
                "sync-composed.json"),
            root);
        await WriteSyncPlanAsync("sync-divergent.json", registrationRevision: 2);
        var rejected = await RunProgramAsync(
            MutationArguments(
                "synchronise-official",
                "sync-divergent-operation",
                "sync-divergent.json"),
            root);

        Assert.Equal((int)AdministrationExitCode.Success, applied.ExitCode);
        Assert.Contains("CH_ADMIN_APPLIED", applied.Output, StringComparison.Ordinal);
        Assert.Equal((int)AdministrationExitCode.Conflict, rejected.ExitCode);
        Assert.Contains(
            "CH_ADMIN_VALIDATION_FAILED",
            rejected.Error,
            StringComparison.Ordinal);
        Assert.Equal(1, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM official_source_snapshots;"));
        Assert.Equal(1, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM source_observations;"));
        Assert.Equal(2, await ScalarAsync(
            options,
            "SELECT COUNT(*) FROM administration_command_journal WHERE status = 'Completed';"));

        async Task WriteSyncPlanAsync(string fileName, long registrationRevision)
        {
            var plan = new
            {
                expectedCatalogueRevision = 1,
                registrationId = registrationId.Value,
                registrationRevision,
                expectedCurrentSnapshotId = (string?)null,
                expectedJournalRevision = 0,
                expectedActivationRevision = 0,
                observationId = "synthetic-observation-1",
                maxAgeSeconds = 86_400,
                currentEtag = (string?)null,
                currentLastModified = (DateTimeOffset?)null,
                document = new
                {
                    databaseProductId = productId.Value,
                    databaseProductRevision = productRevision.Value,
                    documentId = documentId.Value,
                    documentVersion = documentVersion.Value,
                    documentFormat = DocumentFormat.Csv.ToString(),
                    contentLanguage = DocumentContentLanguage.EnGb.ToCanonicalTag(),
                    sourceAdapterId = sourceAdapterId.Value,
                },
                parserPolicy = new
                {
                    maximumByteLength = 4096,
                    maximumUnits = 32,
                    maximumTextCharacters = 4096,
                    maximumFieldsPerRecord = 16,
                    maximumFieldCharacters = 1024,
                },
                chunkingPolicy = new
                {
                    targetScalarCount = 64,
                    overlapScalarCount = 8,
                    hardMaximumScalarCount = 96,
                },
                rights,
            };
            await File.WriteAllTextAsync(
                Path.Combine(root.InputRoot, fileName),
                JsonSerializer.Serialize(plan, PlanJsonOptions));
        }
    }

    [Fact]
    public async Task PersistentOfficialAuthorityResolverReadsCompleteControlAuthority()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var resolver = new SqliteOfficialSourceAuthorityResolver(options);
        var corpusId = new CorpusId("product-authority-corpus");
        var registrationId = new OfficialSourceRegistrationId(
            "postgresql-18-reference-a4-official");

        Assert.Null(await resolver.ResolveAsync(corpusId, registrationId));
        var seed = await SeedOfficialAuthorityAsync(options, includeObservation: true);
        var authority = await resolver.ResolveAsync(seed.CorpusId, seed.Registration.Id);

        Assert.NotNull(authority);
        Assert.Equal(seed.Registration.Id, authority.Registration.Id);
        Assert.Equal(seed.Registration.Revision, authority.Registration.Revision);
        Assert.Equal(
            seed.Registration.CanonicalHttpsUrl,
            authority.Registration.CanonicalHttpsUrl);
        Assert.Equal(seed.Snapshot.Id, authority.CurrentSnapshot!.Id);
        Assert.Equal(seed.Snapshot.ContentObjectId, authority.CurrentSnapshot.ContentObjectId);
        Assert.Equal(1, authority.ObservationJournalRevision);
        Assert.Equal(0, authority.ActivationRevision);
    }

    [Fact]
    public async Task PersistentOfficialAuthorityResolverRejectsIncompleteSnapshotJournal()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var seed = await SeedOfficialAuthorityAsync(options, includeObservation: false);
        var resolver = new SqliteOfficialSourceAuthorityResolver(options);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            resolver.ResolveAsync(seed.CorpusId, seed.Registration.Id));
    }

    [Fact]
    public async Task PersistentOfficialAuthorityResolverRejectsJournalHeadDrift()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var seed = await SeedOfficialAuthorityAsync(options, includeObservation: true);
        await ExecuteSqlAsync(
            options,
            $"""
            UPDATE observation_journal_heads
            SET journal_revision = 2, row_revision = 2
            WHERE corpus_id = '{seed.CorpusId.Value}';
            """);
        var resolver = new SqliteOfficialSourceAuthorityResolver(options);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            resolver.ResolveAsync(seed.CorpusId, seed.Registration.Id));
    }

    [Fact]
    public async Task PersistentOfficialAuthorityResolverRejectsRegistrationRevisionDrift()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var seed = await SeedOfficialAuthorityAsync(options, includeObservation: true);
        var store = new SqliteControlPlaneStore(options);
        var drifted = new OfficialSourceRegistration(
            seed.Registration.Id,
            new SourceRegistrationRevision(2),
            seed.Registration.DatabaseProductId,
            seed.Registration.DocumentId,
            seed.Registration.SourceAdapterId,
            "https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf?revision=2",
            CatalogueItemStatus.Candidate);
        var registrationResult = await store.RegisterOfficialSourceAsync(
            new OfficialSourceRegistrationCommitRequest(
                new OperationId("product-authority-registration-v2"),
                seed.CorpusId,
                drifted,
                Now));
        var resolver = new SqliteOfficialSourceAuthorityResolver(options);

        Assert.Equal(StoreMutationOutcome.Applied, registrationResult.Outcome);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            resolver.ResolveAsync(seed.CorpusId, seed.Registration.Id));
    }

    [Fact]
    public async Task DedicatedLeaseBlocksOtherMutationsAndAllowsItsOwner()
    {
        using var root = TemporaryAdministrationRoot.Create();
        var options = root.CreateStoreOptions();
        await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
        var lease = new SqliteAdministrationLeaseManager(options);
        var corpusId = new CorpusId("lease-corpus");
        var owner = new OperationId("lease-owner");
        Assert.Equal(
            AdministrationLeaseOutcome.Acquired,
            await lease.AcquireAsync(new AdministrationLeaseRequest(
                corpusId,
                owner,
                Now,
                TimeSpan.FromMinutes(5))));
        var snapshot = new CatalogueSnapshot(
            corpusId,
            new CatalogueRevision(1),
            [new DatabaseCategory(new DatabaseCategoryId("category"), "Category")],
            [new DatabaseProduct(
                new DatabaseProductId("database"),
                new DatabaseProductRevision(1),
                "Database",
                CatalogueItemStatus.Candidate,
                [new DatabaseCategoryId("category")])],
            []);
        var store = new SqliteControlPlaneStore(options);

        var blocked = await store.CommitCatalogueAsync(new(
            new OperationId("other-operation"),
            snapshot,
            ExpectedCurrentRevision: 0,
            Now));
        var observationChildBlocked = await store.CommitCatalogueAsync(new(
            AdministrativeChildOperationIds.CreateOfficialObservation(owner),
            snapshot,
            ExpectedCurrentRevision: 0,
            Now));
        var owned = await store.CommitCatalogueAsync(new(
            owner,
            snapshot,
            ExpectedCurrentRevision: 0,
            Now));

        Assert.Equal(StoreMutationOutcome.RetentionConflict, blocked.Outcome);
        Assert.Equal(
            StoreMutationOutcome.RetentionConflict,
            observationChildBlocked.Outcome);
        Assert.Equal(StoreMutationOutcome.Applied, owned.Outcome);
        await lease.ReleaseAsync(corpusId, owner);
    }

    private static async Task<RunResult> RunAsync(
        string[] arguments,
        IConfiguration configuration,
        ILocalOperatingSystemIdentityProvider identity,
        IAdministrationLeaseManager lease,
        IOneShotAdministrativeCommandExecutor executor,
        IAdministrationCommandJournal? journal = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var exitCode = await OneShotAdministrationHost.RunAsync(
            arguments,
            configuration,
            identity,
            lease,
            journal ?? new RecordingJournal(),
            executor,
            output,
            error,
            utcNow ?? (() => Now));
        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private static async Task<RunResult> RunLocalImportAsync(
        TemporaryAdministrationRoot root,
        SqliteStoreOptions options,
        string operationId,
        object plan,
        Func<string, FileAttributes>? readAttributes = null)
    {
        await File.WriteAllTextAsync(
            Path.Combine(root.InputRoot, "import.json"),
            JsonSerializer.Serialize(plan));
        var import = new ImportLocalAdministrativeCommand(
            new ImmutableContentStore(options),
            root.InputRoot,
            readAttributes);
        var executor = new SqliteAdministrativeCommandExecutor(
            new SqliteControlPlaneStore(options),
            importLocal: import);
        return await RunAsync(
            MutationArguments("import-local", operationId, "import.json"),
            Configuration(true, root.InputRoot, root.StoreRoot),
            new StubIdentity("os-sha256:" + new string('a', 64)),
            new RecordingLeaseManager(),
            executor);
    }

    private static void AssertNoContentResidue(SqliteStoreOptions options)
    {
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(options.ContentStoreRoot, "objects"),
            "*",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            Path.Combine(options.ContentStoreRoot, "quarantine"),
            "*",
            SearchOption.AllDirectories));
    }

    private static string Sha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))
            .ToLowerInvariant();

    private static async Task<RunResult> RunProgramAsync(
        string[] arguments,
        TemporaryAdministrationRoot root,
        string? materialisationProfile =
            SyntheticAdministrativeMaterialisationProfile.ProfileName,
        string environment = SyntheticAdministrativeMaterialisationProfile.EnvironmentName,
        bool enableProductProfile = false)
    {
        var executablePath = Path.Combine(
            AppContext.BaseDirectory,
            "RagChallenge.Server.Api.exe");

        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                "The administrative Program host was not copied to the test output.",
                executablePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.Environment.Clear();
        var temporaryDirectory = Path.GetTempPath();
        startInfo.Environment["TEMP"] = temporaryDirectory;
        startInfo.Environment["TMP"] = temporaryDirectory;

        if (OperatingSystem.IsWindows())
        {
            var windowsDirectory = Directory.GetParent(Environment.SystemDirectory)?.FullName ??
                throw new InvalidOperationException("The Windows directory is unavailable.");
            startInfo.Environment["SystemRoot"] = windowsDirectory;
            startInfo.Environment["WINDIR"] = windowsDirectory;
        }

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["DOTNET_ENVIRONMENT"] = environment;
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = environment;
        startInfo.Environment["RagChallenge__Administration__Enabled"] = "true";
        startInfo.Environment["RagChallenge__Administration__StoreRoot"] = root.StoreRoot;
        startInfo.Environment["RagChallenge__Administration__InputRoot"] = root.InputRoot;
        startInfo.Environment.Remove(
            "RagChallenge:Administration:MaterialisationProfile");

        if (materialisationProfile is null)
        {
            startInfo.Environment.Remove(
                "RagChallenge__Administration__MaterialisationProfile");
        }
        else
        {
            startInfo.Environment[
                "RagChallenge__Administration__MaterialisationProfile"] =
                materialisationProfile;
        }

        if (enableProductProfile)
        {
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Enabled"] = "true";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__OfficialSource__Enabled"] = "true";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Rendering__Enabled"] = "true";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Rendering__ProfileId"] = "pdf-page-png-notice-v1";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__Enabled"] = "true";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__ProviderId"] = "openai";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__ModelId"] = "text-embedding-3-small";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__ModelRevision"] = "text-embedding-3-small";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__Dimensions"] = "1536";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__CredentialEnvironmentVariable"] = "RAG_CHALLENGE_TEST_UNSET_CREDENTIAL";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__OperationalAuthorityReference"] = "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-TEST-001";
            startInfo.Environment[
                "RagChallenge__Administration__ProductMaterialisation__Embedding__TrustedOperationalGrantReference"] = "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-TEST-001";
            startInfo.Environment.Remove("RAG_CHALLENGE_TEST_UNSET_CREDENTIAL");
        }

        using var process = Process.Start(startInfo) ??
            throw new InvalidOperationException(
                "The administrative Program host could not be started.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException(
                "The administrative Program host did not finish within the test bound.");
        }

        return new RunResult(
            process.ExitCode,
            await output,
            await error);
    }

    private static IConfiguration ProductProfileConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(ProductProfileValues())
            .Build();

    private static Dictionary<string, string?> ProductProfileValues() =>
        new(StringComparer.Ordinal)
        {
            ["RagChallenge:Administration:ProductMaterialisation:Enabled"] = "true",
            ["RagChallenge:Administration:ProductMaterialisation:OfficialSource:Enabled"] = "true",
            ["RagChallenge:Administration:ProductMaterialisation:Rendering:Enabled"] = "true",
            ["RagChallenge:Administration:ProductMaterialisation:Rendering:ProfileId"] = "pdf-page-png-notice-v1",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:Enabled"] = "true",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:ProviderId"] = "openai",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:ModelId"] = "text-embedding-3-small",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:ModelRevision"] = "text-embedding-3-small",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:Dimensions"] = "1536",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:CredentialEnvironmentVariable"] = "RAG_CHALLENGE_TEST_UNSET_CREDENTIAL",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:OperationalAuthorityReference"] = "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-TEST-001",
            ["RagChallenge:Administration:ProductMaterialisation:Embedding:TrustedOperationalGrantReference"] = "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-TEST-001",
        };

    private static async Task<OfficialAuthoritySeed> SeedOfficialAuthorityAsync(
        SqliteStoreOptions options,
        bool includeObservation)
    {
        var corpusId = new CorpusId("product-authority-corpus");
        var productId = new DatabaseProductId("postgresql-18");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("postgresql-18-reference-a4");
        var documentVersion = new DocumentVersionNumber(1);
        var contentObjectId = new ContentObjectId(
            "cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4");
        var category = new DatabaseCategory(
            new DatabaseCategoryId("relational-database"),
            "Relational database");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "PostgreSQL 18",
            CatalogueItemStatus.Candidate,
            [category.Id]);
        var document = new DocumentVersion(
            documentId,
            documentVersion,
            productId,
            productRevision,
            DocumentFormat.Pdf,
            new DocumentContentLanguage("en"),
            CatalogueItemStatus.Candidate,
            contentObjectId,
            15_771_040,
            ContentMediaType.ApplicationPdf.Value,
            new SourceAdapterId("local-product-intake-pdf-v1"),
            SourceTrustClass.LocalAuthorised,
            sourceDeclaredLanguage: new SourceDeclaredLanguage("en"));
        var store = new SqliteControlPlaneStore(options);
        var catalogueResult = await store.CommitCatalogueAsync(new CatalogueCommitRequest(
            new OperationId("product-authority-catalogue-v1"),
            new CatalogueSnapshot(
                corpusId,
                new CatalogueRevision(1),
                [category],
                [product],
                [document]),
            ExpectedCurrentRevision: 0,
            Now));
        var registration = new OfficialSourceRegistration(
            new OfficialSourceRegistrationId("postgresql-18-reference-a4-official"),
            new SourceRegistrationRevision(1),
            productId,
            documentId,
            new SourceAdapterId("postgresql-official-pdf-v1"),
            "https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf",
            CatalogueItemStatus.Candidate);
        var registrationResult = await store.RegisterOfficialSourceAsync(
            new OfficialSourceRegistrationCommitRequest(
                new OperationId("product-authority-registration-v1"),
                corpusId,
                registration,
                Now));
        var snapshot = new OfficialSourceSnapshot(
            new OfficialSnapshotId("snapshot-" + contentObjectId.Value),
            registration.Id,
            contentObjectId,
            15_771_040,
            ContentMediaType.ApplicationPdf.Value,
            Now);
        var snapshotResult = await store.CommitOfficialSourceAsync(
            new OfficialSourceCommitRequest(
                new OperationId("product-authority-snapshot-v1"),
                corpusId,
                registration,
                snapshot,
                Now));

        Assert.Equal(StoreMutationOutcome.Applied, catalogueResult.Outcome);
        Assert.Equal(StoreMutationOutcome.Applied, registrationResult.Outcome);
        Assert.Equal(StoreMutationOutcome.Applied, snapshotResult.Outcome);

        if (includeObservation)
        {
            var observation = new OfficialSourceObservation(
                new OfficialObservationId("postgresql-18-reference-a4-observation-v1"),
                registration.Id,
                snapshot.Id,
                new ObservationJournalRevision(1),
                OfficialObservationState.Current,
                Now,
                TimeSpan.FromDays(7));
            var observationResult = await store.AppendObservationAsync(
                new ObservationCommitRequest(
                    new OperationId("product-authority-observation-v1"),
                    corpusId,
                    observation,
                    ExpectedJournalRevision: 0,
                    Now));
            Assert.Equal(StoreMutationOutcome.Applied, observationResult.Outcome);
        }

        return new OfficialAuthoritySeed(corpusId, registration, snapshot);
    }

    private static void AssertCanonicalFailure(
        RunResult result,
        AdministrationExitCode expectedExitCode,
        string expectedResultCode)
    {
        Assert.Equal((int)expectedExitCode, result.ExitCode);
        Assert.Contains(expectedResultCode, result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("Sensitive", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static IConfiguration Configuration(
        bool enabled,
        string? inputRoot = null,
        string? storeRoot = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["RagChallenge:Administration:Enabled"] = enabled.ToString(),
                ["RagChallenge:Administration:InputRoot"] = inputRoot,
                ["RagChallenge:Administration:StoreRoot"] = storeRoot,
            }).Build();

    private static string[] StatusArguments(string operationId) =>
    [
        "admin",
        "status",
        "--operation-id",
        operationId,
        "--corpus-id",
        "admin-corpus",
        "--reason",
        "Inspect sanitised local status.",
    ];

    private static string[] MutationArguments(
        string command,
        string operationId,
        string input) =>
    [
        "admin",
        command,
        "--operation-id",
        operationId,
        "--corpus-id",
        "admin-corpus",
        "--reason",
        "Execute a bounded synthetic administration test.",
        "--input",
        input,
    ];

    private static async Task<long> ScalarAsync(SqliteStoreOptions options, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={options.ControlDatabasePath};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<string> TextScalarAsync(
        SqliteStoreOptions options,
        string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={options.ControlDatabasePath};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(
            await command.ExecuteScalarAsync(),
            System.Globalization.CultureInfo.InvariantCulture)!;
    }

    private static async Task ExecuteSqlAsync(SqliteStoreOptions options, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={options.ControlDatabasePath};Mode=ReadWrite;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        _ = await command.ExecuteNonQueryAsync();
    }

    private static RightsPlan CreateTextualRightsPlan(
        DocumentId documentId,
        DocumentVersionNumber documentVersion)
    {
        HashSet<DocumentRight> permitted =
        [
            DocumentRight.SourcePossessionOrDownload,
            DocumentRight.ParsingAndTextualTransformation,
            DocumentRight.Indexing,
            DocumentRight.SourceByteRetention,
            DocumentRight.QuotationAndCitation,
            DocumentRight.AttributionNoticeTrademarkAndChangeMarkingRequirements,
        ];
        return new RightsPlan(
            DocumentRightsEligibilityRecordV1.CurrentSchemaVersion,
            Enum.GetValues<DocumentRight>().Select(right => new RightsDecisionPlan(
                right.ToString(),
                (permitted.Contains(right)
                    ? DocumentRightDecisionState.Permitted
                    : DocumentRightDecisionState.Denied).ToString(),
                $"synthetic-rights-{right}")).ToArray(),
            documentId.Value,
            documentVersion.Value);
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed record RightsPlan(
        int RightsSchemaVersion,
        RightsDecisionPlan[] RightsDecisions,
        string DocumentId,
        long DocumentVersion);

    private sealed record RightsDecisionPlan(
        string Right,
        string State,
        string EvidenceReference);

    private sealed record CataloguePlan(
        string TargetId,
        long? TargetVersion,
        long ExpectedCurrentRevision,
        long Revision,
        CategoryPlan[] Categories,
        DatabaseProductPlan[] DatabaseProducts,
        DocumentPlan[] DocumentVersions);

    private sealed record CategoryPlan(string Id, string DisplayName);

    private sealed record DatabaseProductPlan(
        string Id,
        long Revision,
        string DisplayName,
        string Status,
        string[] CategoryIds);

    private sealed record DocumentPlan(
        string Id,
        long Version,
        string DatabaseProductId,
        long DatabaseProductRevision,
        string Format,
        string ContentLanguage,
        string Status,
        string ContentObjectId,
        long ByteLength,
        string MediaType,
        string SourceAdapterId,
        string SourceTrustClass,
        string? OfficialSourceRegistrationId,
        string? OfficialSnapshotId);

    private sealed record OfficialAuthoritySeed(
        CorpusId CorpusId,
        OfficialSourceRegistration Registration,
        OfficialSourceSnapshot Snapshot);

    private sealed class StubIdentity(string? identifier)
        : ILocalOperatingSystemIdentityProvider
    {
        internal int CallCount { get; private set; }

        public string? GetOpaqueIdentifier()
        {
            CallCount++;
            return identifier;
        }
    }

    private sealed class CountingOfficialSourceTransport(OfficialFetchResult result)
        : IOfficialSourceTransport
    {
        internal int CallCount { get; private set; }

        public Task<OfficialFetchResult> FetchAsync(
            OfficialSourceRegistration registration,
            OfficialFetchPolicy policy,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class NullOfficialSourceAuthorityResolver
        : IOfficialSourceAuthorityResolver
    {
        public Task<OfficialSourceAuthority?> ResolveAsync(
            CorpusId corpusId,
            OfficialSourceRegistrationId registrationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<OfficialSourceAuthority?>(null);
    }

    private sealed class RejectingHttpMessageHandler : HttpMessageHandler
    {
        internal int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("No external HTTP call is allowed in this test.");
        }
    }

    private sealed class CountingEmbeddingProvider(
        EmbeddingProviderDescriptor descriptor)
        : IEmbeddingProvider
    {
        internal int CallCount { get; private set; }

        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var vectors = request.Inputs.Select((input, index) =>
            {
                var vector = new float[descriptor.Dimensions];
                vector[0] = input.Length;

                if (vector.Length > 1)
                {
                    vector[1] = index + 1;
                }

                return (ReadOnlyMemory<float>)vector;
            }).ToArray();
            return Task.FromResult(new EmbeddingBatchResult(descriptor, vectors));
        }
    }

    private sealed class RecordingLeaseManager : IAdministrationLeaseManager
    {
        internal AdministrationLeaseOutcome NextOutcome { get; set; } =
            AdministrationLeaseOutcome.Acquired;

        internal bool FailRelease { get; set; }

        internal Exception? AcquireFailure { get; set; }

        internal List<AdministrationLeaseRequest> Acquisitions { get; } = [];

        internal List<(CorpusId CorpusId, OperationId OperationId)> Releases { get; } = [];

        public Task<AdministrationLeaseOutcome> AcquireAsync(
            AdministrationLeaseRequest request,
            CancellationToken cancellationToken = default)
        {
            Acquisitions.Add(request);

            if (AcquireFailure is not null)
            {
                throw AcquireFailure;
            }

            return Task.FromResult(NextOutcome);
        }

        public Task ReleaseAsync(
            CorpusId corpusId,
            OperationId operationId,
            CancellationToken cancellationToken = default)
        {
            Releases.Add((corpusId, operationId));

            if (FailRelease)
            {
                throw new IOException("Synthetic bounded lease-release failure.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RecordingExecutor : IOneShotAdministrativeCommandExecutor
    {
        internal List<OneShotAdministrativeCommand> Commands { get; } = [];

        internal Exception? ExecuteFailure { get; set; }

        internal JsonElement? ResultPayload { get; set; }

        public AdministrativeCommandIdentifiers DescribeIntent(
            string command,
            CorpusId corpusId,
            JsonElement? input) =>
            new([$"corpus:{corpusId.Value}"], []);

        public Task<AdministrativeExecutionResult> ExecuteAsync(
            OneShotAdministrativeCommand command,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command);

            if (ExecuteFailure is not null)
            {
                throw ExecuteFailure;
            }

            return Task.FromResult(new AdministrativeExecutionResult(
                AdministrativeExecutionOutcome.Applied,
                "CH_ADMIN_APPLIED",
                ResultRevision: 1,
                ResultPayload: ResultPayload));
        }
    }

    private sealed class RecordingJournal : IAdministrationCommandJournal
    {
        private readonly Dictionary<string, (AdministrationJournalIntent Intent,
            AdministrationJournalResult? Result)> entries = new(StringComparer.Ordinal);

        internal Exception? BeginFailure { get; set; }

        public Task<AdministrationJournalBeginResult> BeginAsync(
            AdministrationJournalIntent intent,
            CancellationToken cancellationToken = default)
        {
            if (BeginFailure is not null)
            {
                throw BeginFailure;
            }

            if (!entries.TryGetValue(intent.OperationId.Value, out var entry))
            {
                entries.Add(intent.OperationId.Value, (intent, null));
                return Task.FromResult(new AdministrationJournalBeginResult(
                    AdministrationJournalBeginOutcome.Started,
                    intent.IntentDigest,
                    intent.StartedAt));
            }

            if (!string.Equals(
                    entry.Intent.IntentDigest,
                    intent.IntentDigest,
                    StringComparison.Ordinal))
            {
                throw new AdministrationJournalConflictException(
                    "The operation identity was reused with different intent.");
            }

            return Task.FromResult(new AdministrationJournalBeginResult(
                entry.Result is null
                    ? AdministrationJournalBeginOutcome.Resumed
                    : AdministrationJournalBeginOutcome.CompletedReplay,
                intent.IntentDigest,
                entry.Intent.StartedAt,
                entry.Result));
        }

        public Task CompleteAsync(
            AdministrationJournalCompletion completion,
            DateTimeOffset completedAt,
            CancellationToken cancellationToken = default)
        {
            var entry = entries[completion.OperationId.Value];
            var result = new AdministrationJournalResult(
                completion.Outcome,
                completion.ResultCode,
                completion.ExitCategory,
                completion.ResultRevision,
                completedAt);

            if (entry.Result is not null && entry.Result != result)
            {
                throw new AdministrationJournalConflictException(
                    "The operation result differs from its durable result.");
            }

            entries[completion.OperationId.Value] = (entry.Intent, result);
            return Task.CompletedTask;
        }

        public Task VerifyCompletedAsync(
            AdministrationJournalCompletion completion,
            CancellationToken cancellationToken = default)
        {
            if (!entries.TryGetValue(completion.OperationId.Value, out var entry) ||
                entry.Result is null ||
                entry.Result.Outcome != completion.Outcome ||
                !string.Equals(
                    entry.Result.ResultCode,
                    completion.ResultCode,
                    StringComparison.Ordinal) ||
                entry.Result.ExitCategory != completion.ExitCategory ||
                entry.Result.ResultRevision != completion.ResultRevision)
            {
                throw new AdministrationJournalConflictException(
                    "The completed operation result is absent or different.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class TemporaryAdministrationRoot : IDisposable
    {
        private TemporaryAdministrationRoot(string root)
        {
            Root = root;
            StoreRoot = Path.Combine(root, "store");
            InputRoot = Path.Combine(root, "input");
            Directory.CreateDirectory(StoreRoot);
            Directory.CreateDirectory(InputRoot);
            Directory.CreateDirectory(Path.Combine(StoreRoot, "content"));
        }

        internal string Root { get; }

        internal string StoreRoot { get; }

        internal string InputRoot { get; }

        internal static TemporaryAdministrationRoot Create()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "rag-challenge-admin-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TemporaryAdministrationRoot(root);
        }

        internal SqliteStoreOptions CreateStoreOptions() =>
            new(
                Path.Combine(StoreRoot, "control.db"),
                Path.Combine(StoreRoot, "vectors.db"),
                Path.Combine(StoreRoot, "content"));

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
