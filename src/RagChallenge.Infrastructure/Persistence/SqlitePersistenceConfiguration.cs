// Purpose: Applies the mandatory SQLite durability, integrity, and bounded-locking policy whenever Infrastructure opens a writable persistence connection.
using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace RagChallenge.Infrastructure.Persistence;

internal sealed class SqlitePragmaConnectionInterceptor : DbConnectionInterceptor
{
    private const string ConnectionPragmas = """
        PRAGMA foreign_keys = ON;
        PRAGMA synchronous = FULL;
        PRAGMA trusted_schema = OFF;
        PRAGMA busy_timeout = 5000;
        """;

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        Apply(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ApplyAsync(connection, cancellationToken).ConfigureAwait(false);
        await base.ConnectionOpenedAsync(
            connection,
            eventData,
            cancellationToken).ConfigureAwait(false);
    }

    private static void Apply(DbConnection connection)
    {
        using var journalCommand = connection.CreateCommand();
        journalCommand.CommandText = "PRAGMA journal_mode = WAL;";
        _ = journalCommand.ExecuteScalar();

        using var command = connection.CreateCommand();
        command.CommandText = ConnectionPragmas;
        _ = command.ExecuteNonQuery();
    }

    private static async Task ApplyAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var journalCommand = connection.CreateCommand();
        journalCommand.CommandText = "PRAGMA journal_mode = WAL;";
        _ = await journalCommand
            .ExecuteScalarAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = ConnectionPragmas;
        _ = await command
            .ExecuteNonQueryAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

internal static class SqlitePersistenceOptions
{
    internal static DbContextOptionsBuilder<TContext> Configure<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string databasePath)
        where TContext : DbContext
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath) ??
            throw new ArgumentException(
                "A SQLite database path must have a parent directory.",
                nameof(databasePath));

        Directory.CreateDirectory(directory);

        return builder
            .UseSqlite($"Data Source={fullPath};Mode=ReadWriteCreate;Cache=Private")
            .AddInterceptors(new SqlitePragmaConnectionInterceptor());
    }
}

internal static class DesignTimeStorePath
{
    private const string EnvironmentVariable =
        "RAGCHALLENGE_DESIGN_TIME_STORE_ROOT";

    internal static string Resolve(string fileName)
    {
        var root = Environment.GetEnvironmentVariable(EnvironmentVariable);

        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException(
                $"{EnvironmentVariable} must identify an explicit non-production directory.");
        }

        var fullRoot = Path.GetFullPath(root);
        Directory.CreateDirectory(fullRoot);
        return Path.Combine(fullRoot, fileName);
    }
}
