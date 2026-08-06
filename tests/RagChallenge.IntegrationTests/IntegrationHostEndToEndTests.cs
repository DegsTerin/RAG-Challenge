// Purpose: Verifies the synthetic document-to-answer flow over a real loopback listener and proves durable catalogue, content, activation and index recovery after host restart.
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class IntegrationHostEndToEndTests
{
    [Fact]
    public async Task SameOriginDashboardAndQuerySurviveHostRestart()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "rag-challenge-s06-a-host",
            Guid.NewGuid().ToString("N"));
        var contentRoot = Path.Combine(root, "app");
        var storeRoot = Path.Combine(root, "store");
        Directory.CreateDirectory(Path.Combine(contentRoot, "wwwroot"));
        await File.WriteAllTextAsync(
            Path.Combine(contentRoot, "wwwroot", "index.html"),
            "<!doctype html><html><body><div id=\"root\"></div></body></html>");

        try
        {
            var first = await StartHostAsync(contentRoot, storeRoot);
            Assert.IsType<SyntheticIntegrationRuntime>(
                first.Application.Services.GetRequiredService<IQuestionAnsweringService>());
            string generation;

            try
            {
                using var client = new HttpClient { BaseAddress = first.BaseAddress };
                var shell = await client.GetStringAsync("/");
                Assert.Contains("<div id=\"root\"></div>", shell, StringComparison.Ordinal);

                var response = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", response.GetProperty("outcome").GetString());
                Assert.Equal("en-GB", response.GetProperty("answerLanguage").GetString());
                Assert.Single(response.GetProperty("citations").EnumerateArray());
                generation = response.GetProperty("indexGenerationId").GetString()!;

                var portuguese = await AskAsync(client, "pt-BR");
                Assert.Equal("Answered", portuguese.GetProperty("outcome").GetString());
                Assert.Equal("pt-BR", portuguese.GetProperty("answerLanguage").GetString());
            }
            finally
            {
                await StopHostAsync(first.Application);
            }

            Assert.True(File.Exists(Path.Combine(storeRoot, "control.db")));
            Assert.True(File.Exists(Path.Combine(storeRoot, "vectors.db")));
            Assert.NotEmpty(Directory.EnumerateFiles(
                Path.Combine(storeRoot, "content"),
                "*",
                SearchOption.AllDirectories));

            var second = await StartHostAsync(contentRoot, storeRoot);

            try
            {
                using var client = new HttpClient { BaseAddress = second.BaseAddress };
                var response = await AskAsync(client, "en-GB");
                Assert.Equal("Answered", response.GetProperty("outcome").GetString());
                Assert.Equal(
                    generation,
                    response.GetProperty("indexGenerationId").GetString());

                using var readiness = JsonDocument.Parse(
                    await client.GetStringAsync("/api/v1/health/ready"));
                Assert.Equal("Ready", readiness.RootElement.GetProperty("status").GetString());
                Assert.Equal(
                    generation,
                    readiness.RootElement.GetProperty("activeGenerationId").GetString());
            }
            finally
            {
                await StopHostAsync(second.Application);
            }
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static async Task<JsonElement> AskAsync(HttpClient client, string language)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/questions",
            new
            {
                corpusId = "database-systems-catalogue-mvp",
                questionLanguage = language,
                question = language == "pt-BR"
                    ? "Qual evidência de persistência está disponível?"
                    : "What persistence evidence is available?",
            });
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Synthetic query failed with {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    private static async Task<RunningHost> StartHostAsync(
        string contentRoot,
        string storeRoot)
    {
        var app = SetupHost.Build(
        [
            "--environment", IntegrationRuntimeOptions.EnvironmentName,
            "--contentRoot", contentRoot,
            "--urls", "http://127.0.0.1:0",
            $"--{IntegrationRuntimeOptions.EnabledKey}", "true",
            $"--{IntegrationRuntimeOptions.StoreRootKey}", storeRoot,
            "--RagChallenge:Setup:AllowExternalServices", "false",
        ]);
        await app.StartAsync();
        var addresses = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses ??
            throw new InvalidOperationException("The listener address feature is unavailable.");
        return new RunningHost(app, new Uri(Assert.Single(addresses)));
    }

    private static async Task StopHostAsync(WebApplication app)
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private sealed record RunningHost(WebApplication Application, Uri BaseAddress);
}
