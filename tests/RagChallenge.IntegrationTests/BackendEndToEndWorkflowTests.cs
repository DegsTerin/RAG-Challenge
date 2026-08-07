// Purpose: Proves one synthetic corpus crosses bounded ingestion, durable catalogue, indexing, activation, retrieval, grounded generation and citation reconstruction.
using RagChallenge.Application.Administration;
using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.IntegrationTests;

public sealed class BackendEndToEndWorkflowTests
{
    [Fact]
    public async Task SyntheticCsvCorpusFlowsFromIngestionToGroundedCitation()
    {
        await using var fixture = await SqlitePersistenceFixture.CreateAsync();
        var productId = new DatabaseProductId("db-end-to-end");
        var productRevision = new DatabaseProductRevision(1);
        var documentId = new DocumentId("doc-end-to-end-csv");
        var documentVersion = new DocumentVersionNumber(1);
        var adapterId = new SourceAdapterId("local-csv");
        var ingestion = new DocumentIngestionService(
            fixture.ContentStore,
            [new PdfPigDocumentParser(), new CsvHelperDocumentParser()],
            new DeterministicChunkingStrategy());
        var context = new DocumentChunkingContext(
            SqlitePersistenceFixture.CorpusId,
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            DocumentContentLanguage.EnGb,
            adapterId,
            SourceTrustClass.LocalAuthorised);
        await using var source = new MemoryStream(
            SyntheticParserFixtureFactory.CsvValidQuotedUtf8,
            writable: false);

        var ingested = await ingestion.IngestAsync(new DocumentIngestionRequest(
            source,
            MaximumByteLength: 131_072,
            ContentMediaType.TextCsv,
            new ParserPolicy(131_072, 32, 131_072, 32, 16_384),
            new ChunkingPolicy(128, 16, 160),
            context));

        var category = new DatabaseCategory(
            new DatabaseCategoryId("category-end-to-end"),
            "Synthetic database");
        var product = new DatabaseProduct(
            productId,
            productRevision,
            "End-to-end database",
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
            ingested.Content.ContentObjectId,
            ingested.Content.ByteLength,
            "text/csv",
            adapterId,
            SourceTrustClass.LocalAuthorised);
        var catalogue = new CatalogueSnapshot(
            SqlitePersistenceFixture.CorpusId,
            new CatalogueRevision(1),
            [category],
            [product],
            [document]);
        var administration = new CatalogueAdministrationService(fixture.ControlStore);
        var catalogueResult = await administration.ApplyAsync(
            new CatalogueAdministrationRequest(
                catalogue,
                ExpectedCurrentRevision: 0,
                Audit("catalogue-end-to-end", "add-document", 1)));
        Assert.Equal(StoreMutationOutcome.Applied, catalogueResult.Outcome);

        var binding = new DocumentBinding(
            productId,
            productRevision,
            documentId,
            documentVersion,
            DocumentFormat.Csv,
            adapterId,
            SourceTrustClass.LocalAuthorised);
        var bindings = new[] { binding };
        var embeddingDescriptor = new EmbeddingProviderDescriptor(
            "fake",
            "deterministic-v1",
            "fixture-1",
            dimensions: 3);
        var languageModelDescriptor = new LanguageModelDescriptor(
            "fake",
            "grounded-v1",
            "fixture-1");
        var compatibilityProfile = CreateCompatibilityProfile(embeddingDescriptor);
        var specification = new IndexGenerationSpecification(
            manifestSchemaVersion: 1,
            SqlitePersistenceFixture.CorpusId,
            new CorpusRevision(1),
            new CatalogueRevision(1),
            BindingDigestCanonicalizer.CanonicaliseActiveDocumentSet(bindings).Digest,
            BindingDigestCanonicalizer.CanonicaliseSourceBindingSet(bindings).Digest,
            compatibilityProfile.Key);
        var embedding = new DeterministicEmbeddingProvider(embeddingDescriptor);
        var indexing = new CorpusIndexingService(
            embedding,
            fixture.VectorStore,
            fixture.ControlStore);
        var built = await indexing.BuildAsync(new CorpusIndexingRequest(
            new CandidateBuildId("candidate-end-to-end"),
            specification,
            [new IndexDocumentInput(
                binding,
                DocumentContentLanguage.EnGb,
                ingested.Chunks,
                ingested.ParsedArtifact.ParserDescriptor,
                compatibilityProfile.ChunkingPolicy)],
            embeddingDescriptor,
            compatibilityProfile,
            Audit("generation-end-to-end", "index-generation", 2),
            SqlitePersistenceFixture.At(2)));
        var activation = await new GenerationActivationService(fixture.ControlStore)
            .ActivateAsync(new GenerationActivationRequest(
                built.Manifest,
                bindings,
                ExpectedCurrentRevision: 0,
                SqliteControlPlaneStore.MinimumPreviousGenerationRetention,
                Audit("activation-end-to-end", "activate-generation", 3)));
        Assert.Equal(StoreMutationOutcome.Applied, activation.Outcome);

        var languageModel = new EvidenceCitingLanguageModel(languageModelDescriptor);
        var answering = new QuestionAnsweringService(
            SqlitePersistenceFixture.CorpusId,
            embeddingDescriptor,
            languageModelDescriptor,
            new SqliteQueryActivationReader(fixture.Options),
            embedding,
            fixture.VectorStore,
            languageModel,
            minimumScore: 0.25);
        var result = await answering.AskAsync(
            new QueryRequest(
                SqlitePersistenceFixture.CorpusId,
                SupportedQueryLanguage.EnGb,
                "What evidence is available?",
                "correlation-end-to-end"),
            SqlitePersistenceFixture.At(4));

        var completion = Assert.IsType<QueryCompletion>(result.Completion);
        Assert.Null(result.Failure);
        Assert.Equal(QueryOutcome.Answered, completion.Outcome);
        Assert.Equal("Synthetic grounded answer.", completion.Answer);
        Assert.Equal(built.Manifest.IndexGenerationId, completion.IndexGenerationId);
        var citation = Assert.Single(completion.Citations);
        Assert.Equal(documentId, citation.DocumentId);
        Assert.Equal(DocumentFormat.Csv, citation.DocumentFormat);
        Assert.Equal(DocumentContentLanguage.EnGb, citation.ContentLanguage);
        Assert.Equal(SourceTrustClass.LocalAuthorised, citation.SourceTrustClass);
        Assert.Equal(1, citation.RecordStart);
        Assert.NotEmpty(citation.Columns);
        Assert.Equal(1, completion.EvidenceCoverage.EligibleDatabaseCount);
        Assert.Equal(1, completion.EvidenceCoverage.EligibleDocumentCount);
    }

    private static AdministrativeAuditContext Audit(
        string operationId,
        string command,
        int day) =>
        new(
            new OperationId(operationId),
            "integration-test",
            command,
            "synthetic end-to-end workflow verification",
            SqlitePersistenceFixture.At(day));

    private static IndexCompatibilityProfile CreateCompatibilityProfile(
        EmbeddingProviderDescriptor embeddingDescriptor) =>
        new(
            [
                PdfPigDocumentParser.CompatibilityDescriptor,
                CsvHelperDocumentParser.CompatibilityDescriptor,
            ],
            new ChunkingPolicy(),
            embeddingDescriptor,
            "sqlite-exact-vector-store/1;schema=1;distance=cosine;algorithm=exact-scan;vector=float32");

    private sealed class DeterministicEmbeddingProvider(
        EmbeddingProviderDescriptor descriptor) : IEmbeddingProvider
    {
        public Task<EmbeddingBatchResult> EmbedAsync(
            EmbeddingBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vectors = request.Inputs
                .Select(_ => (ReadOnlyMemory<float>)new float[] { 1, 0, 0 })
                .ToArray();
            return Task.FromResult(new EmbeddingBatchResult(descriptor, vectors));
        }
    }

    private sealed class EvidenceCitingLanguageModel(
        LanguageModelDescriptor descriptor) : ILanguageModel
    {
        public Task<GroundedGenerationResult> GenerateAsync(
            GroundedGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var evidence = Assert.Single(request.Evidence);
            return Task.FromResult(new GroundedGenerationResult(
                descriptor,
                request.QuestionLanguage,
                "Synthetic grounded answer.",
                [evidence.ChunkId]));
        }
    }
}
