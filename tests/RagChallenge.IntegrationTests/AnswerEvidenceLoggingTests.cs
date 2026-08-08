// Purpose: Verifies that the composed answer-evidence logger exposes only the ADR-0010 operational allowlist for successful and failed persistence signals.
using Microsoft.Extensions.Logging;

using RagChallenge.Application.IndexingRetrieval;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;
using RagChallenge.Server.Api.OperationsGovernance;

namespace RagChallenge.IntegrationTests;

public sealed class AnswerEvidenceLoggingTests
{
    [Fact]
    public void StructuredActivityAndLogsRemainInsideTheOperationalAllowlist()
    {
        var logger = new CapturingLogger<SanitisedAnswerEvidenceActivitySink>();
        var sink = new SanitisedAnswerEvidenceActivitySink(logger);
        var activity = new AnswerEvidenceActivity(
            new AnswerEvidenceRecordId(
                "ans-evidence-00000000000000000000000000000001"),
            "correlation-allowlist",
            new CorpusId("main-corpus"),
            new IndexGenerationId($"idxgen-{new string('a', 64)}"),
            CitationCount: 2,
            PageImageCount: 1,
            ElapsedMilliseconds: 17,
            RetentionOutcome: "Applied",
            FailureCode: null);

        sink.Record(activity);
        sink.Record(activity with
        {
            RetentionOutcome = "Failed",
            FailureCode = "CH_UNEXPECTED_FAILURE",
        });

        string[] activityFields =
        [
            "AnswerEvidenceRecordId",
            "CitationCount",
            "CorrelationId",
            "ElapsedMilliseconds",
            "FailureCode",
            "IndexGenerationId",
            "PageImageCount",
            "RetentionOutcome",
            "CorpusId",
        ];
        Assert.Equal(
            activityFields.Order(StringComparer.Ordinal),
            typeof(AnswerEvidenceActivity).GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(2, logger.Entries.Count);
        Assert.Equal(LogLevel.Information, logger.Entries[0].Level);
        Assert.Equal(LogLevel.Warning, logger.Entries[1].Level);
        string[] successfulLogFields =
        [
            "{OriginalFormat}",
            "AnswerEvidenceRecordId",
            "CitationCount",
            "CorrelationId",
            "ElapsedMilliseconds",
            "IndexGenerationId",
            "PageImageCount",
            "RetentionOutcome",
            "CorpusId",
        ];
        Assert.Equal(
            successfulLogFields.Order(StringComparer.Ordinal),
            logger.Entries[0].Fields.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(
            successfulLogFields.Append("FailureCode").Order(StringComparer.Ordinal),
            logger.Entries[1].Fields.Keys.Order(StringComparer.Ordinal));
        Assert.All(logger.Entries, entry => Assert.Null(entry.Exception));
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            var fields = state is IEnumerable<KeyValuePair<string, object?>> structured
                ? structured.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                : throw new InvalidDataException("The answer-evidence log entry is not structured.");
            Entries.Add(new LogEntry(logLevel, eventId, fields, exception));
        }
    }

    private sealed record LogEntry(
        LogLevel Level,
        EventId EventId,
        IReadOnlyDictionary<string, object?> Fields,
        Exception? Exception);
}
