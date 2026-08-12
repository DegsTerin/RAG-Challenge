// Purpose: Implements bounded local catalogue administration with idempotent control-plane commits and digest-only audit context; no public transport is exposed.
using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Administration;

public static class AdministrativeCommands
{
    public static IReadOnlySet<string> Allowed { get; } = new[]
    {
        "add-database",
        "version-database",
        "activate-database",
        "deactivate-database",
        "remove-database",
        "add-document",
        "version-document",
        "activate-document",
        "deactivate-document",
        "remove-document",
        "register-official-source",
        "synchronise-official",
        "render-document",
        "build-index",
        "activate-generation",
        "rollback-generation",
        "status",
    }.ToFrozenSet(StringComparer.Ordinal);

    public static bool IsMutation(string command) =>
        Allowed.Contains(command) && !string.Equals(command, "status", StringComparison.Ordinal);
}

public enum AdministrationLeaseOutcome
{
    Acquired,
    AlreadyOwned,
    Conflict,
}

public sealed record AdministrationLeaseRequest(
    CorpusId CorpusId,
    OperationId OperationId,
    DateTimeOffset AcquiredAt,
    TimeSpan Duration);

public interface IAdministrationLeaseManager
{
    Task<AdministrationLeaseOutcome> AcquireAsync(
        AdministrationLeaseRequest request,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        CorpusId corpusId,
        OperationId operationId,
        CancellationToken cancellationToken = default);
}

public enum AdministrationJournalBeginOutcome
{
    Started,
    Resumed,
    CompletedReplay,
}

public enum AdministrationJournalResultOutcome
{
    Applied,
    Rejected,
    Unavailable,
    Failed,
}

public sealed class AdministrationJournalConflictException(string message)
    : InvalidOperationException(message);

public sealed record AdministrationJournalResult(
    AdministrationJournalResultOutcome Outcome,
    string ResultCode,
    int ExitCategory,
    long? ResultRevision,
    DateTimeOffset CompletedAt);

public sealed record AdministrationJournalBeginResult(
    AdministrationJournalBeginOutcome Outcome,
    string IntentDigest,
    DateTimeOffset StartedAt,
    AdministrationJournalResult? CompletedResult = null);

public sealed class AdministrationJournalIntent
{
    private const int MaximumIdentifiers = 32;
    private const int MaximumIdentifierLength = 160;
    private const int MaximumIdentifiersJsonLength = 4096;

    public AdministrationJournalIntent(
        OperationId operationId,
        CorpusId corpusId,
        string command,
        string actorIdentifier,
        string reasonSha256,
        string? inputSha256,
        IEnumerable<string> sourceIdentifiers,
        IEnumerable<string> targetIdentifiers,
        DateTimeOffset startedAt)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentNullException.ThrowIfNull(corpusId);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorIdentifier);
        ValidateSha256(reasonSha256, nameof(reasonSha256));

        if (!AdministrativeCommands.Allowed.Contains(command) ||
            actorIdentifier.Length > 128 ||
            actorIdentifier.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "The administrative journal command or actor is outside its closed contract.");
        }

        if (inputSha256 is not null)
        {
            ValidateSha256(inputSha256, nameof(inputSha256));
        }

        if (startedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Administrative journal instants must be expressed in UTC.",
                nameof(startedAt));
        }

        OperationId = operationId;
        CorpusId = corpusId;
        Command = command;
        ActorIdentifier = actorIdentifier;
        ReasonSha256 = reasonSha256;
        InputSha256 = inputSha256;
        SourceIdentifiersJson = CanonicaliseIdentifiers(
            sourceIdentifiers,
            nameof(sourceIdentifiers));
        TargetIdentifiersJson = CanonicaliseIdentifiers(
            targetIdentifiers,
            nameof(targetIdentifiers));
        StartedAt = startedAt;
        IntentDigest = Sha256(string.Join(
            '\n',
            operationId.Value,
            corpusId.Value,
            command,
            actorIdentifier,
            reasonSha256,
            inputSha256 ?? "none",
            SourceIdentifiersJson,
            TargetIdentifiersJson));
    }

    public OperationId OperationId { get; }

    public CorpusId CorpusId { get; }

    public string Command { get; }

    public string ActorIdentifier { get; }

    public string ReasonSha256 { get; }

    public string? InputSha256 { get; }

    public string SourceIdentifiersJson { get; }

    public string TargetIdentifiersJson { get; }

    public DateTimeOffset StartedAt { get; }

    public string IntentDigest { get; }

    private static string CanonicaliseIdentifiers(
        IEnumerable<string> identifiers,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(identifiers);
        var values = identifiers
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        if (values.Length > MaximumIdentifiers || values.Any(value =>
                string.IsNullOrWhiteSpace(value) ||
                value.Length > MaximumIdentifierLength ||
                value.Any(character =>
                    !char.IsAsciiLetterOrDigit(character) &&
                    character is not '-' and not '_' and not '.' and not ':')))
        {
            throw new ArgumentException(
                "Administrative journal identifiers must be bounded safe ASCII.",
                parameterName);
        }

        var json = JsonSerializer.Serialize(values);

        if (json.Length > MaximumIdentifiersJsonLength)
        {
            throw new ArgumentException(
                "Administrative journal identifiers exceeded their storage boundary.",
                parameterName);
        }

        return json;
    }

    private static void ValidateSha256(string value, string parameterName)
    {
        if (value.Length != 64 || value.Any(character =>
                character is not (>= '0' and <= '9') and
                    not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "Administrative journal digests must be lowercase SHA-256.",
                parameterName);
        }
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}

public sealed class AdministrationJournalCompletion
{
    private static readonly FrozenSet<int> ExitCategories =
        new[] { 0, 2, 3, 4, 5, 10 }.ToFrozenSet();

    public AdministrationJournalCompletion(
        OperationId operationId,
        string intentDigest,
        AdministrationJournalResultOutcome outcome,
        string resultCode,
        int exitCategory,
        long? resultRevision = null)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(intentDigest);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultCode);

        if (intentDigest.Length != 64 || intentDigest.Any(character =>
                character is not (>= '0' and <= '9') and
                    not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "The journal intent digest must be lowercase SHA-256.",
                nameof(intentDigest));
        }

        if (!Enum.IsDefined(outcome) ||
            !ExitCategories.Contains(exitCategory) ||
            resultRevision < 0 ||
            resultCode.Length > 128 ||
            !resultCode.StartsWith("CH_", StringComparison.Ordinal) ||
            resultCode.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '_'))
        {
            throw new ArgumentException(
                "An administrative journal completion is outside its closed contract.");
        }

        if ((outcome == AdministrationJournalResultOutcome.Applied) !=
            (exitCategory == 0))
        {
            throw new ArgumentException(
                "Only an applied journal outcome may use the success exit category.");
        }

        OperationId = operationId;
        IntentDigest = intentDigest;
        Outcome = outcome;
        ResultCode = resultCode;
        ExitCategory = exitCategory;
        ResultRevision = resultRevision;
    }

    public OperationId OperationId { get; }

    public string IntentDigest { get; }

    public AdministrationJournalResultOutcome Outcome { get; }

    public string ResultCode { get; }

    public int ExitCategory { get; }

    public long? ResultRevision { get; }
}

public interface IAdministrationCommandJournal
{
    Task<AdministrationJournalBeginResult> BeginAsync(
        AdministrationJournalIntent intent,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        AdministrationJournalCompletion completion,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default);

    Task VerifyCompletedAsync(
        AdministrationJournalCompletion completion,
        CancellationToken cancellationToken = default);
}

public sealed class AdministrativeAuditContext
{
    public AdministrativeAuditContext(
        OperationId operationId,
        string actorIdentifier,
        string command,
        string reason,
        DateTimeOffset requestedAt)
    {
        ArgumentNullException.ThrowIfNull(operationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (actorIdentifier.Length > 128 ||
            actorIdentifier.Any(character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not '.' and not '_' and not ':' and not '-'))
        {
            throw new ArgumentException(
                "An administrative actor identifier must be bounded safe ASCII.",
                nameof(actorIdentifier));
        }

        if (command.Length > 64 ||
            command.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '-'))
        {
            throw new ArgumentException(
                "An administrative command must be bounded safe ASCII.",
                nameof(command));
        }

        if (reason.Length > 512 || reason.Any(char.IsControl))
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        if (requestedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Administrative instants must be expressed in UTC.",
                nameof(requestedAt));
        }

        OperationId = operationId;
        ActorIdentifier = actorIdentifier;
        Command = command;
        Reason = reason;
        RequestedAt = requestedAt;
    }

    public OperationId OperationId { get; }

    public string ActorIdentifier { get; }

    public string Command { get; }

    public string Reason { get; }

    public string ReasonSha256 => Sha256(Reason);

    public DateTimeOffset RequestedAt { get; }

    public string CreateDigest(params string[] boundedDetails)
    {
        ArgumentNullException.ThrowIfNull(boundedDetails);

        if (boundedDetails.Any(detail => detail.Length > 512))
        {
            throw new ArgumentException(
                "Administrative audit details must remain bounded.",
                nameof(boundedDetails));
        }

        var material = string.Join(
            '\n',
            ActorIdentifier,
            Command,
            ReasonSha256,
            string.Join('\n', boundedDetails));
        return Sha256(material);
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();
}

public sealed record CatalogueAdministrationRequest(
    CatalogueSnapshot ProposedSnapshot,
    long ExpectedCurrentRevision,
    AdministrativeAuditContext AuditContext,
    string? InputDigest = null,
    AdministrationJournalCompletion? JournalCompletion = null);

public sealed class CatalogueAdministrationService(IControlPlaneStore controlPlaneStore)
{
    private readonly IControlPlaneStore controlPlaneStore =
        controlPlaneStore ?? throw new ArgumentNullException(nameof(controlPlaneStore));

    public Task<StoreMutationResult> ApplyAsync(
        CatalogueAdministrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ProposedSnapshot);
        ArgumentNullException.ThrowIfNull(request.AuditContext);

        if (request.ProposedSnapshot.Revision.Value !=
            request.ExpectedCurrentRevision + 1)
        {
            throw new ArgumentException(
                "The proposed catalogue revision must immediately follow the expected revision.",
                nameof(request));
        }

        var digest = request.AuditContext.CreateDigest(
            request.ProposedSnapshot.CorpusId.Value,
            request.ProposedSnapshot.Revision.ToCanonicalString(),
            request.ProposedSnapshot.DatabaseProducts.Count.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            request.ProposedSnapshot.DocumentVersions.Count.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            request.InputDigest ?? "none");

        return controlPlaneStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                request.AuditContext.OperationId,
                request.ProposedSnapshot,
                request.ExpectedCurrentRevision,
                request.AuditContext.RequestedAt,
                digest,
                request.JournalCompletion),
            cancellationToken);
    }
}
