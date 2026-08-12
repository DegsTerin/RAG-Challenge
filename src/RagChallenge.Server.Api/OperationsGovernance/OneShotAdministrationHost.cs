// Purpose: Runs the accepted local one-shot administration surface with fail-closed configuration, bounded JSON input, OS identity and a per-corpus lease; it never maps HTTP routes.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

using RagChallenge.Application.Administration;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal enum AdministrationExitCode
{
    Success = 0,
    InvalidInput = 2,
    ConfigurationOrAuthorityDenied = 3,
    Conflict = 4,
    DependencyUnavailable = 5,
    UnexpectedFailure = 10,
}

internal enum AdministrativeExecutionOutcome
{
    Applied,
    AlreadyApplied,
    Rejected,
    Unavailable,
}

internal enum AdministrationExecutionPhase
{
    Input,
    Journal,
    Lease,
    Execution,
    Output,
}

internal sealed record AdministrativeExecutionResult(
    AdministrativeExecutionOutcome Outcome,
    string ResultCode,
    long? ResultRevision = null,
    bool JournalCompletionRecorded = false);

internal sealed record AdministrativeCommandIdentifiers(
    IReadOnlyCollection<string> SourceIdentifiers,
    IReadOnlyCollection<string> TargetIdentifiers);

internal sealed record OneShotAdministrativeCommand(
    string Command,
    CorpusId CorpusId,
    OperationId OperationId,
    AdministrativeAuditContext AuditContext,
    JsonElement? Input,
    string? InputSha256,
    string JournalIntentDigest);

internal interface IOneShotAdministrativeCommandExecutor
{
    AdministrativeCommandIdentifiers DescribeIntent(
        string command,
        CorpusId corpusId,
        JsonElement? input);

    Task<AdministrativeExecutionResult> ExecuteAsync(
        OneShotAdministrativeCommand command,
        CancellationToken cancellationToken = default);
}

internal interface ILocalOperatingSystemIdentityProvider
{
    string? GetOpaqueIdentifier();
}

internal static class OneShotAdministrationHost
{
    internal const long MaximumInputBytes = 1_048_576;
    internal static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);

    private const string EnabledKey = "RagChallenge:Administration:Enabled";
    private const string StoreRootKey = "RagChallenge:Administration:StoreRoot";
    private const string InputRootKey = "RagChallenge:Administration:InputRoot";

    internal static bool IsAdministrationMode(string[] args) =>
        args.Length > 0 && string.Equals(args[0], "admin", StringComparison.Ordinal);

    internal static async Task<int> RunProductionAsync(
        string[] args,
        CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return await RunProductionAsync(
            args,
            configuration,
            new LocalOperatingSystemIdentityProvider(),
            Console.Out,
            Console.Error,
            () => DateTimeOffset.UtcNow,
            cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<int> RunProductionAsync(
        string[] args,
        IConfiguration configuration,
        ILocalOperatingSystemIdentityProvider identityProvider,
        TextWriter output,
        TextWriter error,
        Func<DateTimeOffset> utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(identityProvider);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(utcNow);

        if (!configuration.GetValue<bool>(EnabledKey))
        {
            return await RunAsync(
                args,
                configuration,
                identityProvider,
                leaseManager: null,
                journal: null,
                executor: null,
                output,
                error,
                utcNow,
                cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var storeRoot = ResolveConfiguredRoot(
                configuration[StoreRootKey],
                requireExisting: true);
            var options = new SqliteStoreOptions(
                Path.Combine(storeRoot, "control.db"),
                Path.Combine(storeRoot, "vectors.db"),
                Path.Combine(storeRoot, "content"));
            var store = new SqliteControlPlaneStore(options);
            var materialisationPorts =
                AdministrativeMaterialisationProfileResolver.Resolve(
                    configuration,
                    options);
            return await RunAsync(
                args,
                configuration,
                identityProvider,
                new SqliteAdministrationLeaseManager(options),
                new SqliteAdministrationCommandJournal(options),
                AdministrativeMaterialisationComposition.CreateExecutor(
                    options,
                    store,
                    materialisationPorts),
                output,
                error,
                utcNow,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception is ArgumentException or IOException or UnauthorizedAccessException)
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.ConfigurationOrAuthorityDenied,
                "CH_ADMIN_CONFIGURATION_INVALID").ConfigureAwait(false);
            return (int)AdministrationExitCode.ConfigurationOrAuthorityDenied;
        }
    }

    internal static async Task<int> RunAsync(
        string[] args,
        IConfiguration configuration,
        ILocalOperatingSystemIdentityProvider identityProvider,
        IAdministrationLeaseManager? leaseManager,
        IAdministrationCommandJournal? journal,
        IOneShotAdministrativeCommandExecutor? executor,
        TextWriter output,
        TextWriter error,
        Func<DateTimeOffset> utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(identityProvider);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(utcNow);

        if (!TryParseInvocation(args, out var invocation))
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.InvalidInput,
                "CH_ADMIN_INVOCATION_INVALID").ConfigureAwait(false);
            return (int)AdministrationExitCode.InvalidInput;
        }

        if (!configuration.GetValue<bool>(EnabledKey))
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.ConfigurationOrAuthorityDenied,
                "CH_ADMIN_DISABLED").ConfigureAwait(false);
            return (int)AdministrationExitCode.ConfigurationOrAuthorityDenied;
        }

        if (leaseManager is null || journal is null || executor is null)
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.ConfigurationOrAuthorityDenied,
                "CH_ADMIN_CONFIGURATION_INVALID").ConfigureAwait(false);
            return (int)AdministrationExitCode.ConfigurationOrAuthorityDenied;
        }

        string? actor;

        try
        {
            actor = identityProvider.GetOpaqueIdentifier();
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException or
                IOException or UnauthorizedAccessException)
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.ConfigurationOrAuthorityDenied,
                "CH_ADMIN_IDENTITY_UNAVAILABLE").ConfigureAwait(false);
            return (int)AdministrationExitCode.ConfigurationOrAuthorityDenied;
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.ConfigurationOrAuthorityDenied,
                "CH_ADMIN_IDENTITY_UNAVAILABLE").ConfigureAwait(false);
            return (int)AdministrationExitCode.ConfigurationOrAuthorityDenied;
        }

        var requestedAt = utcNow();
        AdministrationJournalIntent? journalIntent = null;
        CorpusId? corpusId = null;
        OperationId? operationId = null;
        var ownsLease = false;
        var phase = AdministrationExecutionPhase.Input;

        try
        {
            corpusId = new CorpusId(invocation.CorpusId);
            operationId = new OperationId(invocation.OperationId);
            var attemptAudit = new AdministrativeAuditContext(
                operationId,
                actor,
                invocation.Command,
                invocation.Reason,
                requestedAt);
            JsonElement? input = null;
            string? inputDigest = null;
            AdministrativeCommandIdentifiers identifiers;

            try
            {
                if (AdministrativeCommands.IsMutation(invocation.Command))
                {
                    var boundedInput = ReadInputBytes(
                        configuration[InputRootKey],
                        invocation.InputPath!);
                    inputDigest = boundedInput.Sha256;
                    input = ParseInput(boundedInput.Bytes);
                }

                identifiers = executor.DescribeIntent(
                    invocation.Command,
                    corpusId,
                    input);
            }
            catch (Exception exception) when (
                exception is ArgumentException or InvalidDataException or JsonException)
            {
                journalIntent = new AdministrationJournalIntent(
                    operationId,
                    corpusId,
                    invocation.Command,
                    actor,
                    attemptAudit.ReasonSha256,
                    inputDigest,
                    FallbackSourceIdentifiers(corpusId, invocation.InputPath),
                    [],
                    requestedAt);
                phase = AdministrationExecutionPhase.Journal;
                var rejectedBegin = await journal.BeginAsync(
                    journalIntent,
                    cancellationToken).ConfigureAwait(false);
                var rejectedAudit = new AdministrativeAuditContext(
                    operationId,
                    actor,
                    invocation.Command,
                    invocation.Reason,
                    rejectedBegin.StartedAt);
                var rejectedCommand = new OneShotAdministrativeCommand(
                    invocation.Command,
                    corpusId,
                    operationId,
                    rejectedAudit,
                    input,
                    inputDigest,
                    rejectedBegin.IntentDigest);

                if (rejectedBegin.Outcome ==
                    AdministrationJournalBeginOutcome.CompletedReplay)
                {
                    return await WriteCompletedReplayAsync(
                        output,
                        error,
                        rejectedCommand,
                        rejectedBegin.CompletedResult!).ConfigureAwait(false);
                }

                var rejection = CreateCompletion(
                    rejectedCommand,
                    AdministrationJournalResultOutcome.Rejected,
                    "CH_ADMIN_INPUT_REJECTED",
                    AdministrationExitCode.InvalidInput);
                phase = AdministrationExecutionPhase.Journal;
                await journal.CompleteAsync(
                    rejection,
                    CompletionInstant(rejectedBegin.StartedAt, utcNow()),
                    cancellationToken).ConfigureAwait(false);
                phase = AdministrationExecutionPhase.Output;
                await WriteFailureAsync(
                    error,
                    AdministrationExitCode.InvalidInput,
                    rejection.ResultCode).ConfigureAwait(false);
                return (int)AdministrationExitCode.InvalidInput;
            }

            journalIntent = new AdministrationJournalIntent(
                operationId,
                corpusId,
                invocation.Command,
                actor,
                attemptAudit.ReasonSha256,
                inputDigest,
                identifiers.SourceIdentifiers,
                identifiers.TargetIdentifiers,
                requestedAt);
            phase = AdministrationExecutionPhase.Journal;
            var journalBegin = await journal.BeginAsync(journalIntent, cancellationToken)
                .ConfigureAwait(false);
            var audit = new AdministrativeAuditContext(
                operationId,
                actor,
                invocation.Command,
                invocation.Reason,
                journalBegin.StartedAt);
            var command = new OneShotAdministrativeCommand(
                invocation.Command,
                corpusId,
                operationId,
                audit,
                input,
                inputDigest,
                journalIntent.IntentDigest);

            if (journalBegin.Outcome == AdministrationJournalBeginOutcome.CompletedReplay)
            {
                phase = AdministrationExecutionPhase.Output;
                return await WriteCompletedReplayAsync(
                    output,
                    error,
                    command,
                    journalBegin.CompletedResult!).ConfigureAwait(false);
            }

            if (AdministrativeCommands.IsMutation(invocation.Command))
            {
                phase = AdministrationExecutionPhase.Lease;
                var lease = await leaseManager.AcquireAsync(
                    new AdministrationLeaseRequest(
                        corpusId,
                        operationId,
                        requestedAt,
                        LeaseDuration),
                    cancellationToken).ConfigureAwait(false);

                if (lease == AdministrationLeaseOutcome.Conflict)
                {
                    var conflict = CreateCompletion(
                        command,
                        AdministrationJournalResultOutcome.Rejected,
                        "CH_ADMIN_LEASE_CONFLICT",
                        AdministrationExitCode.Conflict);
                    phase = AdministrationExecutionPhase.Journal;
                    await journal.CompleteAsync(
                        conflict,
                        CompletionInstant(requestedAt, utcNow()),
                        cancellationToken).ConfigureAwait(false);
                    phase = AdministrationExecutionPhase.Output;
                    await WriteFailureAsync(
                        error,
                        AdministrationExitCode.Conflict,
                        conflict.ResultCode).ConfigureAwait(false);
                    return (int)AdministrationExitCode.Conflict;
                }

                ownsLease = true;
            }

            phase = AdministrationExecutionPhase.Execution;
            var result = await executor.ExecuteAsync(command, cancellationToken)
                .ConfigureAwait(false);
            var exitCode = MapExitCode(result.Outcome);
            var completion = CreateCompletion(command, result, exitCode);

            if (result.JournalCompletionRecorded)
            {
                phase = AdministrationExecutionPhase.Journal;
                await journal.VerifyCompletedAsync(completion, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                phase = AdministrationExecutionPhase.Journal;
                await journal.CompleteAsync(
                    completion,
                    CompletionInstant(requestedAt, utcNow()),
                    cancellationToken).ConfigureAwait(false);
            }

            if (ownsLease)
            {
                try
                {
                    phase = AdministrationExecutionPhase.Lease;
                    await leaseManager.ReleaseAsync(corpusId, operationId, cancellationToken)
                        .ConfigureAwait(false);
                    ownsLease = false;
                }
                catch
                {
                    // The durable command result remains authoritative; the bounded lease
                    // is retried in finally and expires without changing that result.
                }
            }

            if (exitCode == AdministrationExitCode.Success)
            {
                phase = AdministrationExecutionPhase.Output;
                await WriteSuccessAsync(output, command, result).ConfigureAwait(false);
            }
            else
            {
                phase = AdministrationExecutionPhase.Output;
                await WriteFailureAsync(error, exitCode, result.ResultCode)
                    .ConfigureAwait(false);
            }

            return (int)exitCode;
        }
        catch (AdministrationJournalConflictException)
        {
            await WriteFailureAsync(
                error,
                AdministrationExitCode.Conflict,
                "CH_ADMIN_OPERATION_CONFLICT").ConfigureAwait(false);
            return (int)AdministrationExitCode.Conflict;
        }
        catch (Exception exception) when (IsInputFailure(exception, phase))
        {
            await TryCompleteFailureAsync(
                journal,
                journalIntent,
                AdministrationJournalResultOutcome.Rejected,
                "CH_ADMIN_INPUT_REJECTED",
                AdministrationExitCode.InvalidInput,
                utcNow).ConfigureAwait(false);
            await WriteFailureAsync(
                error,
                AdministrationExitCode.InvalidInput,
                "CH_ADMIN_INPUT_REJECTED").ConfigureAwait(false);
            return (int)AdministrationExitCode.InvalidInput;
        }
        catch (InvalidOperationException) when (phase == AdministrationExecutionPhase.Execution)
        {
            await TryCompleteFailureAsync(
                journal,
                journalIntent,
                AdministrationJournalResultOutcome.Rejected,
                "CH_ADMIN_OPERATION_CONFLICT",
                AdministrationExitCode.Conflict,
                utcNow).ConfigureAwait(false);
            await WriteFailureAsync(
                error,
                AdministrationExitCode.Conflict,
                "CH_ADMIN_OPERATION_CONFLICT").ConfigureAwait(false);
            return (int)AdministrationExitCode.Conflict;
        }
        catch (Exception exception) when (IsDependencyFailure(exception, phase))
        {
            await TryCompleteFailureAsync(
                journal,
                journalIntent,
                AdministrationJournalResultOutcome.Unavailable,
                "CH_ADMIN_DEPENDENCY_UNAVAILABLE",
                AdministrationExitCode.DependencyUnavailable,
                utcNow).ConfigureAwait(false);
            await WriteFailureAsync(
                error,
                AdministrationExitCode.DependencyUnavailable,
                "CH_ADMIN_DEPENDENCY_UNAVAILABLE").ConfigureAwait(false);
            return (int)AdministrationExitCode.DependencyUnavailable;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TryCompleteFailureAsync(
                journal,
                journalIntent,
                AdministrationJournalResultOutcome.Failed,
                "CH_ADMIN_CANCELLED",
                AdministrationExitCode.UnexpectedFailure,
                utcNow).ConfigureAwait(false);
            await WriteFailureAsync(
                error,
                AdministrationExitCode.UnexpectedFailure,
                "CH_ADMIN_CANCELLED").ConfigureAwait(false);
            return (int)AdministrationExitCode.UnexpectedFailure;
        }
        catch
        {
            await TryCompleteFailureAsync(
                journal,
                journalIntent,
                AdministrationJournalResultOutcome.Failed,
                "CH_ADMIN_UNEXPECTED_FAILURE",
                AdministrationExitCode.UnexpectedFailure,
                utcNow).ConfigureAwait(false);
            await WriteFailureAsync(
                error,
                AdministrationExitCode.UnexpectedFailure,
                "CH_ADMIN_UNEXPECTED_FAILURE").ConfigureAwait(false);
            return (int)AdministrationExitCode.UnexpectedFailure;
        }
        finally
        {
            if (ownsLease && corpusId is not null && operationId is not null)
            {
                try
                {
                    await leaseManager.ReleaseAsync(
                        corpusId,
                        operationId,
                        CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // The command result remains journalled; lease expiry bounds recovery.
                }
            }
        }
    }

    private static bool IsInputFailure(
        Exception exception,
        AdministrationExecutionPhase phase) =>
        (exception is ArgumentException or InvalidDataException or JsonException &&
            phase is AdministrationExecutionPhase.Input or
                AdministrationExecutionPhase.Execution) ||
        (exception is IOException or UnauthorizedAccessException &&
            phase == AdministrationExecutionPhase.Input);

    private static bool IsDependencyFailure(
        Exception exception,
        AdministrationExecutionPhase phase) =>
        (phase is AdministrationExecutionPhase.Journal or
            AdministrationExecutionPhase.Lease or
            AdministrationExecutionPhase.Execution) &&
        (exception is IOException or UnauthorizedAccessException or TimeoutException ||
            HasSqliteCause(exception));

    private static bool HasSqliteCause(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseInvocation(
        string[] args,
        out AdministrationInvocation invocation)
    {
        invocation = default!;

        if (args.Length < 2 || args.Length > 10 ||
            !string.Equals(args[0], "admin", StringComparison.Ordinal) ||
            !AdministrativeCommands.Allowed.Contains(args[1]) ||
            (args.Length - 2) % 2 != 0)
        {
            return false;
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var index = 2; index < args.Length; index += 2)
        {
            if (args[index] is not ("--operation-id" or "--corpus-id" or "--reason" or
                    "--input") ||
                !values.TryAdd(args[index], args[index + 1]))
            {
                return false;
            }
        }

        if (!values.TryGetValue("--operation-id", out var operationId) ||
            !values.TryGetValue("--corpus-id", out var corpusId) ||
            !values.TryGetValue("--reason", out var reason) ||
            string.IsNullOrWhiteSpace(operationId) ||
            string.IsNullOrWhiteSpace(corpusId) ||
            string.IsNullOrWhiteSpace(reason) ||
            reason.Length > 512 ||
            reason.Any(char.IsControl))
        {
            return false;
        }

        values.TryGetValue("--input", out var inputPath);
        var mutation = AdministrativeCommands.IsMutation(args[1]);

        if (mutation == string.IsNullOrWhiteSpace(inputPath))
        {
            return false;
        }

        invocation = new AdministrationInvocation(
            args[1],
            operationId,
            corpusId,
            reason,
            inputPath);
        return true;
    }

    private static BoundedAdministrativeInput ReadInputBytes(
        string? configuredRoot,
        string relativePath)
    {
        var root = ResolveConfiguredRoot(configuredRoot, requireExisting: true);

        if (Path.IsPathFullyQualified(relativePath) ||
            !string.Equals(Path.GetExtension(relativePath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Administrative input must be one relative JSON file.");
        }

        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        var relative = Path.GetRelativePath(root, path);

        if (Path.IsPathFullyQualified(relative) ||
            relative is "." or ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", PathComparison) ||
            !File.Exists(path))
        {
            throw new InvalidDataException("Administrative input resolved outside its root.");
        }

        EnsureNoReparsePoint(path, root);
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.SequentialScan);
        EnsureNoReparsePoint(path, root);
        var length = stream.Length;

        if (length is <= 0 or > MaximumInputBytes)
        {
            throw new InvalidDataException("Administrative input exceeded its byte limit.");
        }

        var bytes = new byte[checked((int)length)];
        stream.ReadExactly(bytes);

        if (stream.Position != length || stream.Length != length)
        {
            throw new IOException("Administrative input changed while being read.");
        }

        return new BoundedAdministrativeInput(
            bytes,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    private static JsonElement ParseInput(byte[] bytes)
    {
        using var document = JsonDocument.Parse(
            bytes,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 32,
            });
        EnsureNoDuplicateProperties(document.RootElement);
        return document.RootElement.Clone();
    }

    private static List<string> FallbackSourceIdentifiers(
        CorpusId corpusId,
        string? relativeInputPath)
    {
        var identifiers = new List<string> { $"corpus:{corpusId.Value}" };

        if (!string.IsNullOrEmpty(relativeInputPath))
        {
            var pathDigest = SHA256.HashData(Encoding.UTF8.GetBytes(relativeInputPath));
            identifiers.Add(
                $"input-path-sha256:{Convert.ToHexString(pathDigest).ToLowerInvariant()}");
        }

        return identifiers;
    }

    private static string ResolveConfiguredRoot(string? value, bool requireExisting)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value))
        {
            throw new ArgumentException("An administrative root must be explicit.");
        }

        var root = Path.GetFullPath(value)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (root.Length == 0 || Path.GetPathRoot(root) == root ||
            (requireExisting && !Directory.Exists(root)))
        {
            throw new ArgumentException("An administrative root is unavailable.");
        }

        EnsureNoReparsePointToVolumeRoot(root);
        return root;
    }

    private static void EnsureNoDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw new InvalidDataException(
                        "Administrative input cannot contain duplicate JSON properties.");
                }

                EnsureNoDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                EnsureNoDuplicateProperties(item);
            }
        }
    }

    private static void EnsureNoReparsePointToVolumeRoot(string path)
    {
        var current = path;

        while (true)
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    "Administrative paths cannot traverse reparse points.");
            }

            var parent = Path.GetDirectoryName(current);

            if (string.IsNullOrEmpty(parent) || string.Equals(parent, current, PathComparison))
            {
                return;
            }

            current = parent;
        }
    }

    private static void EnsureNoReparsePoint(string path, string root)
    {
        var current = path;

        while (current.StartsWith(root, PathComparison))
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException(
                    "Administrative paths cannot traverse reparse points.");
            }

            if (string.Equals(current, root, PathComparison))
            {
                return;
            }

            current = Path.GetDirectoryName(current) ?? root;
        }

        throw new InvalidDataException("Administrative path containment was not proven.");
    }

    private static AdministrationExitCode MapExitCode(
        AdministrativeExecutionOutcome outcome) =>
        outcome switch
        {
            AdministrativeExecutionOutcome.Applied or
                AdministrativeExecutionOutcome.AlreadyApplied => AdministrationExitCode.Success,
            AdministrativeExecutionOutcome.Rejected => AdministrationExitCode.Conflict,
            AdministrativeExecutionOutcome.Unavailable =>
                AdministrationExitCode.DependencyUnavailable,
            _ => AdministrationExitCode.UnexpectedFailure,
        };

    private static AdministrationJournalCompletion CreateCompletion(
        OneShotAdministrativeCommand command,
        AdministrativeExecutionResult result,
        AdministrationExitCode exitCode) =>
        CreateCompletion(
            command,
            result.Outcome switch
            {
                AdministrativeExecutionOutcome.Applied or
                    AdministrativeExecutionOutcome.AlreadyApplied =>
                        AdministrationJournalResultOutcome.Applied,
                AdministrativeExecutionOutcome.Rejected =>
                    AdministrationJournalResultOutcome.Rejected,
                AdministrativeExecutionOutcome.Unavailable =>
                    AdministrationJournalResultOutcome.Unavailable,
                _ => AdministrationJournalResultOutcome.Failed,
            },
            result.ResultCode,
            exitCode,
            result.ResultRevision);

    private static AdministrationJournalCompletion CreateCompletion(
        OneShotAdministrativeCommand command,
        AdministrationJournalResultOutcome outcome,
        string resultCode,
        AdministrationExitCode exitCode,
        long? resultRevision = null) =>
        new(
            command.OperationId,
            command.JournalIntentDigest,
            outcome,
            resultCode,
            (int)exitCode,
            resultRevision);

    private static async Task<int> WriteCompletedReplayAsync(
        TextWriter output,
        TextWriter error,
        OneShotAdministrativeCommand command,
        AdministrationJournalResult result)
    {
        if (!Enum.IsDefined((AdministrationExitCode)result.ExitCategory))
        {
            throw new InvalidDataException(
                "The administrative journal contains an unknown exit category.");
        }

        var exitCode = (AdministrationExitCode)result.ExitCategory;

        if (exitCode == AdministrationExitCode.Success)
        {
            await WriteSuccessAsync(
                output,
                command,
                new AdministrativeExecutionResult(
                    AdministrativeExecutionOutcome.AlreadyApplied,
                    result.ResultCode,
                    result.ResultRevision,
                    JournalCompletionRecorded: true)).ConfigureAwait(false);
        }
        else
        {
            await WriteFailureAsync(error, exitCode, result.ResultCode)
                .ConfigureAwait(false);
        }

        return result.ExitCategory;
    }

    private static async Task TryCompleteFailureAsync(
        IAdministrationCommandJournal journal,
        AdministrationJournalIntent? intent,
        AdministrationJournalResultOutcome outcome,
        string resultCode,
        AdministrationExitCode exitCode,
        Func<DateTimeOffset> utcNow)
    {
        if (intent is null)
        {
            return;
        }

        try
        {
            await journal.CompleteAsync(
                new AdministrationJournalCompletion(
                    intent.OperationId,
                    intent.IntentDigest,
                    outcome,
                    resultCode,
                    (int)exitCode),
                CompletionInstant(intent.StartedAt, utcNow()),
                CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // A command failure must not be hidden by a secondary journal failure.
        }
    }

    private static DateTimeOffset CompletionInstant(
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        if (completedAt.Offset != TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Administrative completion instants must be expressed in UTC.");
        }

        return completedAt < startedAt ? startedAt : completedAt;
    }

    private static Task WriteSuccessAsync(
        TextWriter output,
        OneShotAdministrativeCommand command,
        AdministrativeExecutionResult result) =>
        output.WriteLineAsync(JsonSerializer.Serialize(new
        {
            status = result.Outcome.ToString(),
            resultCode = result.ResultCode,
            command = command.Command,
            operationId = command.OperationId.Value,
            corpusId = command.CorpusId.Value,
            resultRevision = result.ResultRevision,
        }));

    private static Task WriteFailureAsync(
        TextWriter error,
        AdministrationExitCode exitCode,
        string resultCode) =>
        error.WriteLineAsync(JsonSerializer.Serialize(new
        {
            status = "Failed",
            resultCode,
            exitCode = (int)exitCode,
        }));

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private sealed record AdministrationInvocation(
        string Command,
        string OperationId,
        string CorpusId,
        string Reason,
        string? InputPath);

    private sealed record BoundedAdministrativeInput(byte[] Bytes, string Sha256);

    private sealed class LocalOperatingSystemIdentityProvider
        : ILocalOperatingSystemIdentityProvider
    {
        public string? GetOpaqueIdentifier()
        {
            var identity = $"{Environment.UserDomainName}\0{Environment.UserName}";

            if (string.IsNullOrWhiteSpace(identity.Replace("\0", "", StringComparison.Ordinal)))
            {
                return null;
            }

            var digest = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
            return $"os-sha256:{Convert.ToHexString(digest).ToLowerInvariant()}";
        }
    }
}
