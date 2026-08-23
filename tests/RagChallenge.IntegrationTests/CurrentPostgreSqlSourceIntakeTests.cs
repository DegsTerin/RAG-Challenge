// Purpose: Performs the explicit local-only identity, parser and chunk-count preflight for the current PostgreSQL 18.4 source intake without credentials or provider access.
using System.Security.Cryptography;

using RagChallenge.Application.Documents;
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Infrastructure.Providers;

namespace RagChallenge.IntegrationTests;

public sealed class CurrentPostgreSqlSourceIntakeTests
{
    private const string OptInVariable = "RAG_CHALLENGE_CURRENT_SOURCE_INTAKE_TEST";
    private const string ExpectedSha256 =
        "cea7b845568095eb56dee1b51bfa145c6c6637bc4377c986019971577efefae4";

    [Fact]
    [Trait("Category", "LocalCurrentProduct")]
    public async Task CurrentSourceIntakeProducesExactlyThreeThousandTwoHundredAndEightyTwoChunks()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(OptInVariable),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        var repositoryRoot = Path.GetFullPath(
            Environment.GetEnvironmentVariable("RAG_CHALLENGE_REPOSITORY_ROOT") ??
            throw new InvalidOperationException("The explicit repository root is absent."));
        var path = Path.GetFullPath(
            Environment.GetEnvironmentVariable("RAG_CHALLENGE_CURRENT_SOURCE_INTAKE_PATH") ??
            throw new InvalidOperationException("The explicit source intake path is absent."));
        Assert.StartsWith(
            repositoryRoot + Path.DirectorySeparatorChar,
            path,
            StringComparison.OrdinalIgnoreCase);
        var file = new FileInfo(path);
        Assert.True(file.Exists);
        Assert.Equal(15_771_040, file.Length);
        Assert.False(file.Attributes.HasFlag(FileAttributes.ReparsePoint));

        await using var content = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1_048_576,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var observedSha256 = Convert.ToHexString(await SHA256.HashDataAsync(content))
            .ToLowerInvariant();
        Assert.Equal(ExpectedSha256, observedSha256);
        content.Position = 0;
        var parsed = await new PdfPigDocumentParser().ParseAsync(
            content,
            new ParserPolicy(
                maximumByteLength: 33_554_432,
                maximumUnits: 5_000,
                maximumTextCharacters: 16_000_000));
        var chunks = new DeterministicChunkingStrategy().Chunk(
            parsed,
            new DocumentChunkingContext(
                new CorpusId("rag-challenge-product"),
                new DatabaseProductId("postgresql-18"),
                new DatabaseProductRevision(1),
                new DocumentId("postgresql-18-reference-a4"),
                new DocumentVersionNumber(1),
                DocumentFormat.Pdf,
                new DocumentContentLanguage("en"),
                new SourceAdapterId("local-authorised-pdf-v1"),
                SourceTrustClass.LocalAuthorised),
            new ChunkingPolicy());
        var embeddingDescriptor = new EmbeddingProviderDescriptor(
            OpenAiEmbeddingCostSchedule.ProviderId,
            OpenAiEmbeddingCostSchedule.ModelId,
            OpenAiEmbeddingCostSchedule.ModelId,
            dimensions: 1536);
        var requests = chunks.Chunk(64)
            .Select(batch => new EmbeddingBatchRequest(
                embeddingDescriptor,
                batch.Select(chunk => chunk.Text).ToArray(),
                maximumUtf8Bytes: 1_048_576))
            .ToArray();
        var exactRequestBytes = requests
            .Select(OpenAiHttpEmbeddingProvider.SerialiseRequest)
            .ToArray();
        var conservativeMaximumMicroUsd = exactRequestBytes.Sum(bytes =>
            OpenAiEmbeddingCostSchedule.CalculateMaximumMicroUsd(bytes.Length));

        Assert.Equal(PdfPigDocumentParser.CompatibilityDescriptor, parsed.ParserDescriptor);
        Assert.Equal(3_282, chunks.Count);
        Assert.Equal(52, requests.Length);
        Assert.All(requests.Take(51), request => Assert.Equal(64, request.Inputs.Count));
        Assert.Equal(18, requests[^1].Inputs.Count);
        Assert.InRange(conservativeMaximumMicroUsd, 1, 1_000_000);
        Console.WriteLine(
            $"conservativeMaximumMicroUsd={conservativeMaximumMicroUsd};" +
            $"exactRequestJsonBytes={exactRequestBytes.Sum(bytes => (long)bytes.Length)}");
    }
}
