// Purpose: Defines explicit local store paths and the one-shot migration boundary without auto-migrating during normal runtime construction.
using Microsoft.EntityFrameworkCore;

namespace RagChallenge.Infrastructure.Persistence;

public sealed class SqliteStoreOptions
{
    public SqliteStoreOptions(
        string controlDatabasePath,
        string vectorDatabasePath,
        string contentStoreRoot)
    {
        ControlDatabasePath = StoragePathSafety.ResolveDatabasePath(
            controlDatabasePath,
            nameof(controlDatabasePath));
        VectorDatabasePath = StoragePathSafety.ResolveDatabasePath(
            vectorDatabasePath,
            nameof(vectorDatabasePath));
        ContentStoreRoot = StoragePathSafety.ResolveRootPath(
            contentStoreRoot,
            nameof(contentStoreRoot));

        if (string.Equals(
                ControlDatabasePath,
                VectorDatabasePath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "control.db and vectors.db must use distinct physical paths.",
                nameof(vectorDatabasePath));
        }
    }

    public string ControlDatabasePath { get; }

    public string VectorDatabasePath { get; }

    public string ContentStoreRoot { get; }

    internal ControlPlaneDbContext CreateControlContext()
    {
        var options = new DbContextOptionsBuilder<ControlPlaneDbContext>()
            .Configure(ControlDatabasePath)
            .Options;
        return new ControlPlaneDbContext(options);
    }

    internal VectorStoreDbContext CreateVectorContext()
    {
        var options = new DbContextOptionsBuilder<VectorStoreDbContext>()
            .Configure(VectorDatabasePath)
            .Options;
        return new VectorStoreDbContext(options);
    }
}

public static class SqliteStoreProvisioner
{
    public static async Task ApplyMigrationsAsync(
        SqliteStoreOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await using var controlContext = options.CreateControlContext();
        await controlContext.Database
            .MigrateAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var vectorContext = options.CreateVectorContext();
        await vectorContext.Database
            .MigrateAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

internal static class StoragePathSafety
{
    internal static string ResolveDatabasePath(string path, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        var fullPath = Path.GetFullPath(path);
        var parent = Path.GetDirectoryName(fullPath) ??
            throw new ArgumentException(
                "A database path must have a parent directory.",
                parameterName);

        EnsureExistingPathIsNotReparsePoint(parent, parameterName);

        if (File.Exists(fullPath))
        {
            EnsureExistingPathIsNotReparsePoint(fullPath, parameterName);
        }

        return fullPath;
    }

    internal static string ResolveRootPath(string path, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        var fullPath = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (fullPath.Length == 0 || Path.GetPathRoot(fullPath) == fullPath)
        {
            throw new ArgumentException(
                "A storage root cannot be a filesystem root.",
                parameterName);
        }

        EnsureExistingPathIsNotReparsePoint(fullPath, parameterName);
        return fullPath;
    }

    internal static string CombineUnderRoot(
        string root,
        params string[] pathSegments)
    {
        var combined = Path.GetFullPath(Path.Combine([root, .. pathSegments]));
        var prefix = root + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(prefix, PathComparison))
        {
            throw new InvalidOperationException(
                "A storage path resolved outside its configured root.");
        }

        return combined;
    }

    internal static void EnsureExistingPathIsNotReparsePoint(
        string path,
        string parameterName)
    {
        var current = Path.GetFullPath(path);

        if (!Directory.Exists(current) && !File.Exists(current))
        {
            current = Path.GetDirectoryName(current) ?? string.Empty;
        }

        while (current.Length > 0)
        {
            if (Directory.Exists(current) || File.Exists(current))
            {
                var attributes = File.GetAttributes(current);

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new ArgumentException(
                        "Storage paths cannot traverse a reparse point.",
                        parameterName);
                }
            }

            current = Path.GetDirectoryName(
                current.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)) ?? string.Empty;
        }
    }

    internal static StringComparison PathComparison =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
