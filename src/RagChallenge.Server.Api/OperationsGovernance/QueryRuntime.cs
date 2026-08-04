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
