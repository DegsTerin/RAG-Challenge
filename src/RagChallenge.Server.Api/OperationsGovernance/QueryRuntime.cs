// Purpose: Supplies fail-closed query composition and the process-local concurrency ceiling without enabling provider, source or cloud access.
using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Server.Api.Contracts.V1;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed class QueryConcurrencyGate : IDisposable
{
    private readonly SemaphoreSlim semaphore = new(initialCount: 20, maxCount: 20);

    internal async Task<bool> TryEnterAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await semaphore.WaitAsync(TimeSpan.Zero, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    internal void Exit() => semaphore.Release();

    public void Dispose() => semaphore.Dispose();
}

internal sealed class DisabledQuestionAnsweringService : IQuestionAnsweringService
{
    public Task<QueryExecutionResult> AskAsync(
        QueryRequest request,
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(QueryExecutionResult.Failed(new QueryFailure(
            QueryFailureKind.ConfigurationInvalid,
            request.CorrelationId)));
}

internal sealed partial class SanitisedAnswerEvidenceActivitySink(
    ILogger<SanitisedAnswerEvidenceActivitySink> logger) : IAnswerEvidenceActivitySink
{
    private readonly ILogger<SanitisedAnswerEvidenceActivitySink> logger = logger ??
        throw new ArgumentNullException(nameof(logger));

    public void Record(AnswerEvidenceActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (activity.FailureCode is null)
        {
            LogPersisted(
                logger,
                activity.AnswerEvidenceRecordId.Value,
                activity.CorrelationId,
                activity.CorpusId.Value,
                activity.IndexGenerationId.Value,
                activity.CitationCount,
                activity.PageImageCount,
                activity.ElapsedMilliseconds,
                activity.RetentionOutcome);
            return;
        }

        LogFailed(
            logger,
            activity.AnswerEvidenceRecordId.Value,
            activity.CorrelationId,
            activity.CorpusId.Value,
            activity.IndexGenerationId.Value,
            activity.CitationCount,
            activity.PageImageCount,
            activity.ElapsedMilliseconds,
            activity.RetentionOutcome,
            activity.FailureCode);
    }

    [LoggerMessage(
        EventId = 7041,
        Level = LogLevel.Information,
        Message = "Answer evidence {AnswerEvidenceRecordId} for correlation {CorrelationId}, corpus {CorpusId}, generation {IndexGenerationId}: {CitationCount} citations, {PageImageCount} pages, {ElapsedMilliseconds} ms, retention {RetentionOutcome}")]
    private static partial void LogPersisted(
        ILogger logger,
        string answerEvidenceRecordId,
        string correlationId,
        string corpusId,
        string indexGenerationId,
        int citationCount,
        int pageImageCount,
        long elapsedMilliseconds,
        string retentionOutcome);

    [LoggerMessage(
        EventId = 7042,
        Level = LogLevel.Warning,
        Message = "Answer evidence {AnswerEvidenceRecordId} for correlation {CorrelationId}, corpus {CorpusId}, generation {IndexGenerationId}: {CitationCount} citations, {PageImageCount} pages, {ElapsedMilliseconds} ms, retention {RetentionOutcome}, failure {FailureCode}")]
    private static partial void LogFailed(
        ILogger logger,
        string answerEvidenceRecordId,
        string correlationId,
        string corpusId,
        string indexGenerationId,
        int citationCount,
        int pageImageCount,
        long elapsedMilliseconds,
        string retentionOutcome,
        string failureCode);
}

internal interface IQueryReadinessProbe
{
    ValueTask<ReadinessV1> CheckAsync(
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default);
}

internal sealed class DisabledQueryReadinessProbe : IQueryReadinessProbe
{
    private static readonly IReadOnlyCollection<SanitisedCapabilityCheckV1> Checks =
        new[] { new SanitisedCapabilityCheckV1("query-runtime", "Unavailable") };

    public ValueTask<ReadinessV1> CheckAsync(
        DateTimeOffset observedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new ReadinessV1(
            "Unready",
            ActiveDatabaseCount: 0,
            EligibleDocumentCount: 0,
            DegradedDocumentCount: 0,
            SourceStates: Array.Empty<SanitisedSourceStateV1>(),
            ActiveGenerationId: null,
            ConfigurationRevision: "unconfigured",
            Checks,
            observedAt));
    }
}

internal sealed class ApiExceptionHandler : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var kind = exception switch
        {
            BadHttpRequestException => QueryFailureKind.InvalidInput,
            OperationCanceledException => QueryFailureKind.OperationCancelled,
            _ => QueryFailureKind.UnexpectedFailure,
        };
        await QueryEndpoints.Problem(kind, httpContext.TraceIdentifier)
            .ExecuteAsync(httpContext).ConfigureAwait(false);
        return true;
    }
}
