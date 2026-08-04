// Purpose: Implements bounded local catalogue administration with idempotent control-plane commits and digest-only audit context; no public transport is exposed.
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Application.Administration;

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

        if (reason.Length > 512)
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

        var reasonDigest = Sha256(Reason);
        var material = string.Join(
            '\n',
            ActorIdentifier,
            Command,
            reasonDigest,
            RequestedAt.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
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
    AdministrativeAuditContext AuditContext);

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
                System.Globalization.CultureInfo.InvariantCulture));

        return controlPlaneStore.CommitCatalogueAsync(
            new CatalogueCommitRequest(
                request.AuditContext.OperationId,
                request.ProposedSnapshot,
                request.ExpectedCurrentRevision,
                request.AuditContext.RequestedAt,
                digest),
            cancellationToken);
    }
}
