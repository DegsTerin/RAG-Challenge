// Purpose: Verifies the Oracle-only product composition, persisted readiness checks and fail-closed migration path without contacting external providers.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.Administration;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Persistence;
using RagChallenge.Infrastructure.Providers;
using RagChallenge.Server.Api.Contracts.V1;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class ProductQueryRuntimeTests
{
    [Fact]
    public void PostgreSqlAuthorityAcceptsOnlyTheExactLocalAuthorisedDocumentProfile()
    {
        var valid = ProductRuntimeFixture.CreatePostgreSqlAuthority();
        ProductQueryRuntime.ValidatePostgreSql18Authority(
            valid.Catalogue,
            valid.Activation,
            ProductRuntimeFixture.PostgreSqlRightsReference);
        var staleCatalogueRevision = ProductRuntimeFixture.CreatePostgreSqlAuthority(
            catalogueRevision: 4);
        var officialSubstitution = ProductRuntimeFixture.CreatePostgreSqlAuthority(
            officialDocument: true);

        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidatePostgreSql18Authority(
                valid.Catalogue,
                valid.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidatePostgreSql18Authority(
                staleCatalogueRevision.Catalogue,
                staleCatalogueRevision.Activation,
                ProductRuntimeFixture.PostgreSqlRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidatePostgreSql18Authority(
                officialSubstitution.Catalogue,
                officialSubstitution.Activation,
                ProductRuntimeFixture.PostgreSqlRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateConfiguredAuthority(
                valid.Catalogue,
                valid.Activation,
                ProductRuntimeFixture.PostgreSqlRightsReference,
                ProductCatalogueProfile.OracleDatabase19c));
    }

    [Fact]
    public void OracleOnlyAuthorityAcceptsOnlyTheExactCatalogueAndDocumentProfile()
    {
        var valid = ProductRuntimeFixture.CreateAuthority();
        ProductQueryRuntime.ValidateOracleOnlyAuthority(
            valid.Catalogue,
            valid.Activation,
            ProductRuntimeFixture.ApprovedRightsReference);

        var oracleCandidate = ProductRuntimeFixture.CreateAuthority(
            oracleStatus: CatalogueItemStatus.Candidate);
        var otherDeactivated = ProductRuntimeFixture.CreateAuthority(
            otherStatus: CatalogueItemStatus.Deactivated);
        var revisionDrift = ProductRuntimeFixture.CreateAuthority(
            activationCatalogueRevision: 52);
        var catalogueRevisionDrift = ProductRuntimeFixture.CreateAuthority(
            catalogueRevision: 52,
            activationCatalogueRevision: 52);
        var missingProduct = ProductRuntimeFixture.CreateAuthority(removeLastProduct: true);
        var substitutedProduct = ProductRuntimeFixture.CreateAuthority(
            substituteLastProductId: "future-database");
        var substitutedDocument = ProductRuntimeFixture.CreateAuthority(
            documentId: "oracle-database-19c-other");
        var substitutedContent = ProductRuntimeFixture.CreateAuthority(
            contentObjectId: new ContentObjectId(new string('b', 64)));
        var substitutedLength = ProductRuntimeFixture.CreateAuthority(
            byteLength: 9_322_920);
        var substitutedFormat = ProductRuntimeFixture.CreateAuthority(
            documentFormat: DocumentFormat.Csv);
        var unapprovedRights = ProductRuntimeFixture.CreateAuthority(
            rightsEvidenceReference: "unapproved-oracle-rights-evidence");

        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                oracleCandidate.Catalogue,
                oracleCandidate.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                otherDeactivated.Catalogue,
                otherDeactivated.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                revisionDrift.Catalogue,
                revisionDrift.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                catalogueRevisionDrift.Catalogue,
                catalogueRevisionDrift.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                missingProduct.Catalogue,
                missingProduct.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                substitutedProduct.Catalogue,
                substitutedProduct.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                substitutedDocument.Catalogue,
                substitutedDocument.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                substitutedContent.Catalogue,
                substitutedContent.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                substitutedLength.Catalogue,
                substitutedLength.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                substitutedFormat.Catalogue,
                substitutedFormat.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
        Assert.Throws<InvalidDataException>(() =>
            ProductQueryRuntime.ValidateOracleOnlyAuthority(
                unapprovedRights.Catalogue,
                unapprovedRights.Activation,
                ProductRuntimeFixture.ApprovedRightsReference));
    }

    [Fact]
    public void ProductOptionsRejectUnapprovedRightsBeforeCredentialLookup()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-product-options-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var settings = new Dictionary<string, string?>
            {
                [ProductQueryRuntimeOptions.EnabledKey] = "true",
                [ProductQueryRuntimeOptions.StoreRootKey] = root,
                [ProductQueryRuntimeOptions.CatalogueProfileKey] = "oracle-database-19c",
                [ProductQueryRuntimeOptions.CredentialKey] = "MISSING_PRODUCT_TEST_KEY",
                [ProductQueryRuntimeOptions.QueryEmbeddingAuthorityKey] =
                    "AUTH-QUERY-EMBEDDING-TEST-001",
                [ProductQueryRuntimeOptions.GroundedGenerationAuthorityKey] =
                    "AUTH-GROUNDED-GENERATION-TEST-001",
            };
            var missing = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            var missingError = Assert.Throws<InvalidOperationException>(() =>
                ProductQueryRuntimeOptions.Resolve(missing));

            settings[ProductQueryRuntimeOptions.ApprovedRightsEvidenceKey] =
                ProductQueryRuntimeOptions.SupersededUnverifiedRightsEvidenceReference;
            var superseded = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            var supersededError = Assert.Throws<InvalidOperationException>(() =>
                ProductQueryRuntimeOptions.Resolve(superseded));

            Assert.Contains("approved product rights", missingError.Message, StringComparison.Ordinal);
            Assert.Contains("not approved", supersededError.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ConfiguredProductRuntimeAppliesMigrationsAndFailsReadinessClosed()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-product-runtime-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        const string credentialName = "RAG_CHALLENGE_TEST_PRODUCT_CREDENTIAL";
        WebApplication? app = null;

        try
        {
            app = SetupHost.Build(
            [
                $"--{ProductQueryRuntimeOptions.EnabledKey}", "true",
                $"--{ProductQueryRuntimeOptions.ApplyMigrationsKey}", "true",
                $"--{ProductQueryRuntimeOptions.StoreRootKey}", root,
                $"--{ProductQueryRuntimeOptions.CatalogueProfileKey}", "oracle-database-19c",
                $"--{ProductQueryRuntimeOptions.ApprovedRightsEvidenceKey}",
                ProductRuntimeFixture.ApprovedRightsReference.Value,
                $"--{ProductQueryRuntimeOptions.CredentialKey}", credentialName,
                $"--{ProductQueryRuntimeOptions.QueryEmbeddingAuthorityKey}",
                "AUTH-QUERY-EMBEDDING-TEST-001",
                $"--{ProductQueryRuntimeOptions.GroundedGenerationAuthorityKey}",
                "AUTH-GROUNDED-GENERATION-TEST-001",
                "--RagChallenge:Setup:AllowExternalServices", "true",
            ]);
            var probe = Assert.IsType<ProductQueryRuntime>(
                app.Services.GetRequiredService<IQueryReadinessProbe>());

            var readiness = await probe.CheckAsync(ProductRuntimeFixture.ObservedAt);
            var context = new DefaultHttpContext
            {
                RequestServices = app.Services,
            };
            context.Response.Body = new MemoryStream();
            var result = await HealthEndpoints.ReadyAsync(probe, CancellationToken.None);
            await result.ExecuteAsync(context);

            Assert.Equal("Unready", readiness.Status);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            Assert.Equal(
                1,
                await ScalarAsync(
                    Path.Combine(root, "control.db"),
                    "SELECT COUNT(*) FROM __EFMigrationsHistory " +
                    "WHERE MigrationId = '20260813004642_AllowAnswerEvidenceCitationBcp47Language';"));
        }
        finally
        {
            if (app is not null)
            {
                await app.DisposeAsync();
            }

            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ProductReadinessRejectsAPersistedNonOracleProfile()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-product-runtime-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var options = new SqliteStoreOptions(
            Path.Combine(root, "control.db"),
            Path.Combine(root, "vectors.db"),
            Path.Combine(root, "content"));
        const string credentialName = "RAG_CHALLENGE_TEST_PRODUCT_CREDENTIAL";
        var credentialReads = 0;

        try
        {
            await SqliteStoreProvisioner.ApplyMigrationsAsync(options);
            await ProductRuntimeFixture.SeedInvalidButOtherwiseQueryableStoreAsync(options);
            using var runtime = new ProductQueryRuntime(new ProductQueryRuntimeOptions(
                options,
                ProductCatalogueProfile.OracleDatabase19c,
                ProductRuntimeFixture.ApprovedRightsReference,
                credentialName,
                ProductProviderOperationalAuthority.Parse(
                    ProductProviderOperation.QueryEmbedding,
                    "AUTH-QUERY-EMBEDDING-TEST-001"),
                ProductProviderOperationalAuthority.Parse(
                    ProductProviderOperation.GroundedGeneration,
                    "AUTH-GROUNDED-GENERATION-TEST-001"),
                ProductProviderOperationalGrantSet.DenyAll(),
                ApplyMigrations: false),
                credentialEnvironmentReader: _ =>
                {
                    credentialReads++;
                    return "synthetic-product-credential";
                });

            var readiness = await runtime.CheckAsync(ProductRuntimeFixture.ObservedAt);

            Assert.Equal("Unready", readiness.Status);
            Assert.Equal(
                "Unavailable",
                Assert.Single(readiness.Checks).State);
            Assert.Equal(0, credentialReads);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task SwappedProviderAuthorityStopsBeforeCredentialLookupAndHttpDispatch()
    {
        var credentialReads = 0;
        var handler = new CountingHttpMessageHandler();
        using var client = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };
        var swappedAuthority = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.GroundedGeneration,
            "AUTH-GROUNDED-GENERATION-TEST-001");
        var trustedGrants = new ProductProviderOperationalGrantSet([swappedAuthority]);
        var source = new ProductProviderCredentialSource(
            swappedAuthority,
            trustedGrants,
            ProductProviderOperation.QueryEmbedding,
            "RAG_CHALLENGE_TEST_PRODUCT_CREDENTIAL",
            _ =>
            {
                credentialReads++;
                return "synthetic-product-credential";
            });
        var provider = new OpenAiHttpEmbeddingProvider(client, source.ReadAsync);

        var exception = await Assert.ThrowsAsync<ProductProviderOperationalAuthorityException>(() =>
            provider.EmbedAsync(new EmbeddingBatchRequest(
                new EmbeddingProviderDescriptor(
                    "openai",
                    "text-embedding-3-small",
                    "text-embedding-3-small",
                    dimensions: 3),
                ["synthetic question"],
                maximumUtf8Bytes: 4096)));

        Assert.Contains(
            ProductProviderOperationalAuthorityException.StopCode,
            exception.Message,
            StringComparison.Ordinal);
        Assert.Equal(0, credentialReads);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task RequestedConfigurationDoesNotCreateATrustedOperationalGrant()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-product-grant-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var credentialReads = 0;
        var handler = new CountingHttpMessageHandler();
        using var client = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ProductQueryRuntimeOptions.EnabledKey] = "true",
                    [ProductQueryRuntimeOptions.StoreRootKey] = root,
                    [ProductQueryRuntimeOptions.CatalogueProfileKey] = "oracle-database-19c",
                    [ProductQueryRuntimeOptions.ApprovedRightsEvidenceKey] =
                        ProductRuntimeFixture.ApprovedRightsReference.Value,
                    [ProductQueryRuntimeOptions.CredentialKey] =
                        "RAG_CHALLENGE_TEST_PRODUCT_CREDENTIAL",
                    [ProductQueryRuntimeOptions.QueryEmbeddingAuthorityKey] =
                        "AUTH-QUERY-EMBEDDING-REQUESTED-001",
                    [ProductQueryRuntimeOptions.GroundedGenerationAuthorityKey] =
                        "AUTH-GROUNDED-GENERATION-REQUESTED-001",
                })
                .Build();
            var options = Assert.IsType<ProductQueryRuntimeOptions>(
                ProductQueryRuntimeOptions.Resolve(configuration));
            var source = new ProductProviderCredentialSource(
                options.QueryEmbeddingAuthority,
                options.OperationalGrants,
                ProductProviderOperation.QueryEmbedding,
                options.CredentialEnvironmentVariable,
                _ =>
                {
                    credentialReads++;
                    return "synthetic-product-credential";
                });
            var provider = new OpenAiHttpEmbeddingProvider(client, source.ReadAsync);

            await Assert.ThrowsAsync<ProductProviderOperationalAuthorityException>(() =>
                provider.EmbedAsync(new EmbeddingBatchRequest(
                    new EmbeddingProviderDescriptor(
                        "openai",
                        "text-embedding-3-small",
                        "text-embedding-3-small",
                        dimensions: 3),
                    ["synthetic question"],
                    maximumUtf8Bytes: 4096)));

            Assert.Equal(0, credentialReads);
            Assert.Equal(0, handler.CallCount);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task UntrustedGrantStopsBeforeCredentialLookupAndHttpDispatch()
    {
        var credentialReads = 0;
        var handler = new CountingHttpMessageHandler();
        using var client = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.openai.com/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(25),
        };
        var requested = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.QueryEmbedding,
            "AUTH-QUERY-EMBEDDING-REQUESTED-001");
        var differentTrustedGrant = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.QueryEmbedding,
            "AUTH-QUERY-EMBEDDING-TRUSTED-OTHER-001");
        var source = new ProductProviderCredentialSource(
            requested,
            new ProductProviderOperationalGrantSet([differentTrustedGrant]),
            ProductProviderOperation.QueryEmbedding,
            "RAG_CHALLENGE_TEST_PRODUCT_CREDENTIAL",
            _ =>
            {
                credentialReads++;
                return "synthetic-product-credential";
            });
        var provider = new OpenAiHttpEmbeddingProvider(client, source.ReadAsync);

        await Assert.ThrowsAsync<ProductProviderOperationalAuthorityException>(() =>
            provider.EmbedAsync(new EmbeddingBatchRequest(
                new EmbeddingProviderDescriptor(
                    "openai",
                    "text-embedding-3-small",
                    "text-embedding-3-small",
                    dimensions: 3),
                ["synthetic question"],
                maximumUtf8Bytes: 4096)));

        Assert.Equal(0, credentialReads);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("auth-lowercase")]
    [InlineData("AUTH-")]
    [InlineData("AUTH-A-")]
    [InlineData("AUTH--INVALID")]
    [InlineData("AUTH-INVALID_VALUE")]
    [InlineData("AUTH-GROUNDED-GENERATION-TEST-001")]
    public void OperationalAuthorityRequiresABoundedAuthReference(string? reference)
    {
        Assert.Throws<ArgumentException>(() =>
            ProductProviderOperationalAuthority.Parse(
                ProductProviderOperation.QueryEmbedding,
                reference));
    }

    [Fact]
    public void OperationalAuthorityReferencesAreDistinctAcrossAllThreeOperations()
    {
        var administrative = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.AdministrativeIndexEmbedding,
            "AUTH-ADMINISTRATIVE-INDEX-EMBEDDING-TEST-001");
        var query = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.QueryEmbedding,
            "AUTH-QUERY-EMBEDDING-TEST-001");
        var generation = ProductProviderOperationalAuthority.Parse(
            ProductProviderOperation.GroundedGeneration,
            "AUTH-GROUNDED-GENERATION-TEST-001");
        var grants = new ProductProviderOperationalGrantSet(
            [administrative, query, generation]);

        grants.Demand(
            administrative,
            ProductProviderOperation.AdministrativeIndexEmbedding);
        grants.Demand(query, ProductProviderOperation.QueryEmbedding);
        grants.Demand(generation, ProductProviderOperation.GroundedGeneration);
        Assert.Throws<ArgumentException>(() =>
            ProductProviderOperationalAuthority.Parse(
                ProductProviderOperation.QueryEmbedding,
                administrative.Reference));
        Assert.Throws<ArgumentException>(() =>
            ProductProviderOperationalAuthority.Parse(
                ProductProviderOperation.GroundedGeneration,
                query.Reference));
    }

    private static async Task<long> ScalarAsync(string path, string sql)
    {
        await using var connection = new SqliteConnection(
            $"Data Source={path};Mode=ReadOnly;Cache=Private");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(
            await command.ExecuteScalarAsync(),
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class CountingHttpMessageHandler : HttpMessageHandler
    {
        internal int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("HTTP dispatch was not expected.");
        }
    }

    private static class ProductRuntimeFixture
    {
        internal static DateTimeOffset ObservedAt { get; } =
            new(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);

        internal static DocumentRightsEvidenceReference ApprovedRightsReference { get; } =
            new("approved-oracle-rights-evidence");

        internal static DocumentRightsEvidenceReference PostgreSqlRightsReference { get; } =
            new("auth-s07-a-product-a0-003");

        internal static ProductAuthority CreatePostgreSqlAuthority(
            long catalogueRevision = 3,
            bool officialDocument = false)
        {
            var category = new DatabaseCategory(
                new DatabaseCategoryId("relational-database"),
                "Relational database");
            var productId = new DatabaseProductId("postgresql-18");
            var productRevision = new DatabaseProductRevision(1);
            var documentId = new DocumentId("postgresql-18-reference-a4");
            var contentObjectId = new ContentObjectId(
                "cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4");
            var registrationId = new OfficialSourceRegistrationId(
                "postgresql-18-reference-a4-official");
            var snapshotId = new OfficialSnapshotId(
                "snapshot-cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4");
            var document = new DocumentVersion(
                documentId,
                new DocumentVersionNumber(1),
                productId,
                productRevision,
                DocumentFormat.Pdf,
                new DocumentContentLanguage("en"),
                CatalogueItemStatus.Active,
                contentObjectId,
                15_771_040,
                "application/pdf",
                new SourceAdapterId(officialDocument
                    ? "postgresql-official-pdf-v1"
                    : "local-authorised-pdf-v1"),
                officialDocument
                    ? SourceTrustClass.OfficialExternal
                    : SourceTrustClass.LocalAuthorised,
                officialDocument ? registrationId : null,
                officialDocument ? snapshotId : null,
                sourceDeclaredLanguage: new SourceDeclaredLanguage("en"));
            var catalogue = new CatalogueSnapshot(
                ProductQueryRuntime.CorpusId,
                new CatalogueRevision(catalogueRevision),
                [category],
                [new DatabaseProduct(
                    productId,
                    productRevision,
                    "PostgreSQL 18",
                    CatalogueItemStatus.Active,
                    [category.Id])],
                [document]);
            var binding = new DocumentBinding(
                productId,
                productRevision,
                document.Id,
                document.Version,
                document.Format,
                document.SourceAdapterId,
                document.SourceTrustClass,
                officialDocument ? registrationId : null,
                officialDocument ? snapshotId : null,
                officialDocument
                    ? new OfficialObservationId(
                        "postgresql-18-reference-a4-observation-v1")
                    : null);
            var rights = new DocumentRightsEligibilityRecordV1(
                document.Id,
                document.Version,
                Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                    right,
                    right == DocumentRight.SourceAndDerivativeByteDistributionOrPublication
                        ? DocumentRightDecisionState.Denied
                        : DocumentRightDecisionState.Permitted,
                    PostgreSqlRightsReference)));
            var evidence = new DocumentActivationEvidenceBinding(
                binding,
                contentObjectId,
                rights,
                new RenderManifestId($"rendermanifest-{Hash("postgresql-18.4-render")}"));
            var activation = new CorpusActivationRecord(
                ProductQueryRuntime.CorpusId,
                new ActivationRecordRevision(1),
                previousRecordRevision: null,
                new IndexGenerationId($"idxgen-{Hash("postgresql-18.4-generation")}"),
                catalogue.Revision,
                new ActivationBindingSetDigest(Hash("postgresql-18.4-activation")),
                [binding],
                ObservedAt,
                ObservedAt,
                [evidence]);
            return new ProductAuthority(catalogue, activation, binding, evidence);
        }

        internal static ProductAuthority CreateAuthority(
            CatalogueItemStatus oracleStatus = CatalogueItemStatus.Active,
            CatalogueItemStatus otherStatus = CatalogueItemStatus.Candidate,
            long catalogueRevision = 53,
            long activationCatalogueRevision = 53,
            bool removeLastProduct = false,
            string? substituteLastProductId = null,
            string documentId = "oracle-database-19c-concepts",
            ContentObjectId? contentObjectId = null,
            DocumentFormat documentFormat = DocumentFormat.Pdf,
            long byteLength = 9_322_921,
            string rightsEvidenceReference = "approved-oracle-rights-evidence")
        {
            var categoriesAndProducts = ReadCanonicalCatalogue(
                oracleStatus,
                otherStatus,
                removeLastProduct,
                substituteLastProductId);
            var oracleId = new DatabaseProductId("oracle-database");
            var document = new DocumentVersion(
                new DocumentId(documentId),
                new DocumentVersionNumber(1),
                oracleId,
                new DatabaseProductRevision(1),
                documentFormat,
                new DocumentContentLanguage("en"),
                oracleStatus == CatalogueItemStatus.Active
                    ? CatalogueItemStatus.Active
                    : CatalogueItemStatus.Candidate,
                contentObjectId ?? new ContentObjectId(
                    "6a10b7840c42a1dd6ea9b69337532ed3f903d17af24f144c2a104b925f6533d2"),
                byteLength,
                documentFormat == DocumentFormat.Pdf ? "application/pdf" : "text/csv",
                new SourceAdapterId(documentFormat == DocumentFormat.Pdf
                    ? "local-authorised-pdf-v1"
                    : "local-authorised-csv-v1"),
                SourceTrustClass.LocalAuthorised,
                sourceDeclaredLanguage: new SourceDeclaredLanguage("en"));
            var catalogue = new CatalogueSnapshot(
                ProductQueryRuntime.CorpusId,
                new CatalogueRevision(catalogueRevision),
                categoriesAndProducts.Categories,
                categoriesAndProducts.Products,
                [document]);
            var binding = new DocumentBinding(
                document.DatabaseProductId,
                document.DatabaseProductRevision,
                document.Id,
                document.Version,
                document.Format,
                document.SourceAdapterId,
                document.SourceTrustClass);
            var rights = new DocumentRightsEligibilityRecordV1(
                document.Id,
                document.Version,
                Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                    right,
                    DocumentRightDecisionState.Permitted,
                    new DocumentRightsEvidenceReference(rightsEvidenceReference))));
            var evidence = new DocumentActivationEvidenceBinding(
                binding,
                document.ContentObjectId,
                rights,
                renderManifestId: documentFormat == DocumentFormat.Pdf
                    ? new RenderManifestId($"rendermanifest-{Hash("oracle-19c-render")}")
                    : null);
            var activation = new CorpusActivationRecord(
                ProductQueryRuntime.CorpusId,
                new ActivationRecordRevision(1),
                previousRecordRevision: null,
                new IndexGenerationId($"idxgen-{Hash("oracle-product-generation")}"),
                new CatalogueRevision(activationCatalogueRevision),
                new ActivationBindingSetDigest(Hash("oracle-product-activation")),
                [binding],
                ObservedAt,
                ObservedAt,
                [evidence]);
            return new ProductAuthority(catalogue, activation, binding, evidence);
        }

        internal static async Task SeedInvalidButOtherwiseQueryableStoreAsync(
            SqliteStoreOptions options)
        {
            var bytes = Encoding.UTF8.GetBytes("topic,value\nversion,19c\n");
            var contentStore = new ImmutableContentStore(options);
            await using var source = new MemoryStream(bytes, writable: false);
            var content = await contentStore.PutAndVerifyAsync(new BoundedContentInput(
                source,
                bytes.Length,
                ContentMediaType.TextCsv));
            var category = new DatabaseCategory(
                new DatabaseCategoryId("relational-sql"),
                "Relational (SQL)");
            var productId = new DatabaseProductId("oracle-database");
            var productRevision = new DatabaseProductRevision(1);
            var document = new DocumentVersion(
                new DocumentId("oracle-database-19c-invalid-test"),
                new DocumentVersionNumber(1),
                productId,
                productRevision,
                DocumentFormat.Csv,
                new DocumentContentLanguage("en"),
                CatalogueItemStatus.Active,
                content.ContentObjectId,
                content.ByteLength,
                ContentMediaType.TextCsv.Value,
                new SourceAdapterId("local-authorised-csv-v1"),
                SourceTrustClass.LocalAuthorised,
                sourceDeclaredLanguage: new SourceDeclaredLanguage("en"));
            var catalogue = new CatalogueSnapshot(
                ProductQueryRuntime.CorpusId,
                new CatalogueRevision(1),
                [category],
                [new DatabaseProduct(
                    productId,
                    productRevision,
                    "Oracle Database",
                    CatalogueItemStatus.Active,
                    [category.Id])],
                [document]);
            var binding = new DocumentBinding(
                productId,
                productRevision,
                document.Id,
                document.Version,
                document.Format,
                document.SourceAdapterId,
                document.SourceTrustClass);
            var controlStore = new SqliteControlPlaneStore(options);
            Assert.Equal(
                StoreMutationOutcome.Applied,
                (await controlStore.CommitCatalogueAsync(new CatalogueCommitRequest(
                    new OperationId("invalid-product-catalogue"),
                    catalogue,
                    ExpectedCurrentRevision: 0,
                    ObservedAt))).Outcome);

            var vectorStore = new SqliteVectorIndexStore(options);
            var candidateId = new CandidateBuildId("invalid-product-candidate");
            var vector = new float[
                ProductAdministrativeMaterialisationProfile.AcceptedEmbeddingDimensions];
            vector[0] = 1;
            await vectorStore.CreateCandidateAsync(
                candidateId,
                ProductQueryRuntime.CorpusId,
                ProductAdministrativeMaterialisationProfile.CompatibilityProfile.Key,
                vector.Length,
                expectedChunkCount: 1,
                ObservedAt);
            await vectorStore.AddChunksAsync(candidateId, [new VectorChunkWrite(
                0,
                document.Id,
                document.Version,
                new LogicalArtifactDigest(Hash("invalid-product-chunk")),
                "Synthetic invalid product runtime chunk.",
                vector,
                document.ContentLanguage,
                RecordNumber: 1)]);
            var specification = new IndexGenerationSpecification(
                manifestSchemaVersion: 1,
                ProductQueryRuntime.CorpusId,
                new CorpusRevision(1),
                catalogue.Revision,
                BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet([binding]).Digest,
                BindingDigestCanonicalizer.CanonicaliseSourceBindingSet([binding]).Digest,
                ProductAdministrativeMaterialisationProfile.CompatibilityProfile.Key);
            var generation = await vectorStore.FinaliseCandidateAsync(
                candidateId,
                specification,
                ObservedAt);
            Assert.Equal(
                StoreMutationOutcome.Applied,
                (await controlStore.CommitGenerationAsync(new GenerationCommitRequest(
                    new OperationId("invalid-product-generation"),
                    candidateId,
                    generation,
                    [binding],
                    ObservedAt))).Outcome);
            var rights = new DocumentRightsEligibilityRecordV1(
                document.Id,
                document.Version,
                Enum.GetValues<DocumentRight>().Select(right => new DocumentRightDecision(
                    right,
                    DocumentRightDecisionState.Permitted,
                    new DocumentRightsEvidenceReference($"invalid-product-{right}"))));
            var evidence = new DocumentActivationEvidenceBinding(
                binding,
                document.ContentObjectId,
                rights,
                renderManifestId: null);
            var activation = await new GenerationActivationService(controlStore).ActivateAsync(
                new GenerationActivationRequest(
                    generation,
                    [evidence],
                    ExpectedCurrentRevision: 0,
                    SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                    new AdministrativeAuditContext(
                        new OperationId("invalid-product-activation"),
                        "integration-test",
                        "activate-generation",
                        "synthetic invalid product readiness fixture",
                        ObservedAt)));
            Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);
        }

        private static CatalogueSeed ReadCanonicalCatalogue(
            CatalogueItemStatus oracleStatus,
            CatalogueItemStatus otherStatus,
            bool removeLastProduct,
            string? substituteLastProductId)
        {
            var path = Path.Combine(
                FindRepositoryRoot(),
                "tests",
                "RagChallenge.UnitTests",
                "TestData",
                "initial-catalogue-v1.json");
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            var categories = json.RootElement.GetProperty("categories")
                .EnumerateArray()
                .Select(item => new DatabaseCategory(
                    new DatabaseCategoryId(item.GetProperty("id").GetString()!),
                    item.GetProperty("displayName").GetString()!))
                .ToArray();
            var categoryIds = categories.ToDictionary(
                category => category.Id.Value,
                category => category.Id,
                StringComparer.Ordinal);
            var productElements = json.RootElement.GetProperty("products")
                .EnumerateArray()
                .ToArray();
            if (removeLastProduct)
            {
                productElements = productElements[..^1];
            }

            var lastProductId = productElements[^1].GetProperty("id").GetString()!;

            var firstOtherId = productElements
                .Select(item => item.GetProperty("id").GetString()!)
                .First(id => !string.Equals(id, "oracle-database", StringComparison.Ordinal));
            var products = productElements.Select(item =>
            {
                var originalId = item.GetProperty("id").GetString()!;
                var id = substituteLastProductId is not null &&
                    string.Equals(originalId, lastProductId, StringComparison.Ordinal)
                        ? substituteLastProductId
                        : originalId;
                var status = string.Equals(id, "oracle-database", StringComparison.Ordinal)
                    ? oracleStatus
                    : string.Equals(id, firstOtherId, StringComparison.Ordinal)
                        ? otherStatus
                        : CatalogueItemStatus.Candidate;
                return new DatabaseProduct(
                    new DatabaseProductId(id),
                    new DatabaseProductRevision(1),
                    item.GetProperty("displayName").GetString()!,
                    status,
                    item.GetProperty("categoryIds")
                        .EnumerateArray()
                        .Select(category => categoryIds[category.GetString()!]))!;
            }).ToArray();
            return new CatalogueSeed(categories, products);
        }

        private static string FindRepositoryRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "RAG-Challenge.sln")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException(
                "The RAG-Challenge repository root could not be located.");
        }

        private static string Hash(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
                .ToLowerInvariant();

        internal sealed record ProductAuthority(
            CatalogueSnapshot Catalogue,
            CorpusActivationRecord Activation,
            DocumentBinding Binding,
            DocumentActivationEvidenceBinding Evidence);

        private sealed record CatalogueSeed(
            IReadOnlyCollection<DatabaseCategory> Categories,
            IReadOnlyCollection<DatabaseProduct> Products);
    }
}
