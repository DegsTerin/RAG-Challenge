// Purpose: Imports one explicitly bounded local PDF or CSV into the immutable content store while containing all filesystem access beneath the configured administrative input root.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed class ImportLocalAdministrativeCommand : IAdministrativeMaterialisationCommand
{
    private const long MaximumSupportedBytes = 512L * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly IDocumentContentStore contentStore;
    private readonly string inputRoot;
    private readonly Func<string, FileAttributes> readAttributes;

    internal ImportLocalAdministrativeCommand(
        IDocumentContentStore contentStore,
        string inputRoot,
        Func<string, FileAttributes>? readAttributes = null)
    {
        this.contentStore = contentStore ?? throw new ArgumentNullException(nameof(contentStore));
        this.inputRoot = ResolveRoot(inputRoot);
        this.readAttributes = readAttributes ?? File.GetAttributes;
    }

    public string CommandName => "import-local";

    public AdministrativeCommandIdentifiers DescribeIntent(
        CorpusId corpusId,
        JsonElement? input)
    {
        ArgumentNullException.ThrowIfNull(corpusId);
        var payload = ReadPayload(input);
        var pathDigest = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(payload.RelativePath)))
            .ToLowerInvariant();
        return new AdministrativeCommandIdentifiers(
            [$"local-input-path-sha256:{pathDigest}"],
            [$"content-object:{payload.ExpectedSha256}"]);
    }

    public async Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var payload = ReadPayload(command.Input);
        var path = ResolveFile(payload.RelativePath, payload.MediaType);
        var expectedId = new ContentObjectId(payload.ExpectedSha256);
        var mediaType = new ContentMediaType(payload.MediaType);

        await using var source = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        EnsureRegularFile(path);
        var descriptor = await contentStore.PutAndVerifyAsync(
            new BoundedContentInput(
                source,
                payload.MaximumByteLength,
                mediaType,
                expectedId),
            cancellationToken).ConfigureAwait(false);
        var resultPayload = JsonSerializer.SerializeToElement(new
        {
            contentObjectId = descriptor.ContentObjectId.Value,
            sha256 = descriptor.Sha256.Value,
            byteLength = descriptor.ByteLength,
            mediaType = descriptor.MediaType.Value,
            writeOutcome = descriptor.WriteOutcome.ToString(),
        });
        return new AdministrativeExecutionResult(
            descriptor.WriteOutcome == ContentObjectWriteOutcome.Published
                ? AdministrativeExecutionOutcome.Applied
                : AdministrativeExecutionOutcome.AlreadyApplied,
            "CH_ADMIN_APPLIED",
            ResultPayload: resultPayload);
    }

    private static ImportLocalPayload ReadPayload(JsonElement? input)
    {
        if (input is null)
        {
            throw new InvalidDataException("Local import requires one input payload.");
        }

        var payload = input.Value.Deserialize<ImportLocalPayload>(JsonOptions) ??
            throw new InvalidDataException("Local import input is unavailable.");
        if (string.IsNullOrWhiteSpace(payload.RelativePath) ||
            Path.IsPathFullyQualified(payload.RelativePath) ||
            payload.MaximumByteLength is <= 0 or > MaximumSupportedBytes ||
            string.IsNullOrWhiteSpace(payload.ExpectedSha256) ||
            string.IsNullOrWhiteSpace(payload.MediaType))
        {
            throw new InvalidDataException("Local import input is invalid.");
        }

        _ = new ContentObjectId(payload.ExpectedSha256);
        _ = new ContentMediaType(payload.MediaType);
        return payload;
    }

    private string ResolveFile(string relativePath, string mediaType)
    {
        var expectedExtension = mediaType switch
        {
            "application/pdf" => ".pdf",
            "text/csv" or "application/csv" => ".csv",
            _ => throw new InvalidDataException("Local import supports only PDF and CSV."),
        };
        if (!string.Equals(Path.GetExtension(relativePath), expectedExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Local import media type and extension differ.");
        }

        var path = Path.GetFullPath(Path.Combine(inputRoot, relativePath));
        var relative = Path.GetRelativePath(inputRoot, path);
        if (Path.IsPathFullyQualified(relative) ||
            relative is "." or ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", PathComparison) ||
            !File.Exists(path))
        {
            throw new InvalidDataException("Local import resolved outside its input root.");
        }

        EnsureRegularFile(path);
        return path;
    }

    private void EnsureRegularFile(string path)
    {
        var current = new DirectoryInfo(Path.GetDirectoryName(path)!);
        while (current is not null &&
               !string.Equals(current.FullName, inputRoot, PathComparison))
        {
            if ((readAttributes(current.FullName) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException("Local import cannot traverse a reparse point.");
            }

            current = current.Parent;
        }

        var attributes = readAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
        {
            throw new InvalidDataException("Local import requires one regular file.");
        }
    }

    private static string ResolveRoot(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value))
        {
            throw new ArgumentException("The local input root must be explicit.", nameof(value));
        }

        var root = Path.GetFullPath(value).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(root) || Path.GetPathRoot(root) == root ||
            (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
        {
            throw new ArgumentException("The local input root is unavailable.", nameof(value));
        }

        return root;
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private sealed class ImportLocalPayload
    {
        public required string RelativePath { get; init; }

        public required long MaximumByteLength { get; init; }

        public required string MediaType { get; init; }

        public required string ExpectedSha256 { get; init; }
    }
}
