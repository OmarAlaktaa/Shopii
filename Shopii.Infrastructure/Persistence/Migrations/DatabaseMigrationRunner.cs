using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shopii.Infrastructure.Persistence.Migrations;

/// <summary>
/// Applies the hand-authored SQL migrations that this project keeps next to the
/// EF model. The database was authored separately from EF, so there is no
/// __EFMigrationsHistory table; each migration is responsible for its own
/// idempotency and the runner detects the resulting schema before applying it.
/// </summary>
public static class DatabaseMigrationRunner
{
    private const string UsersEmailMigration =
        "Shopii.Infrastructure.Persistence.Migrations.001_users_email_partial_unique.sql";

    private const string EmailIndexName = "IX_Users_Email_Live";
    private const string DeletedAtIndexName = "IX_Users_DeletedAt";

    public static async Task ApplyDatabaseMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ShopiiDbContext>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseMigrationRunner));

        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException(
                "The Shopii database connection string was not configured.");

        var dataSource = new SqliteConnectionStringBuilder(connectionString)
            .DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) ||
            string.Equals(dataSource, ":memory:", StringComparison.Ordinal))
        {
            logger.LogInformation(
                "Skipping SQL migrations for an in-memory SQLite database.");
            return;
        }

        var databasePath = Path.GetFullPath(dataSource);

        if (!File.Exists(databasePath))
        {
            logger.LogWarning(
                "SQLite database '{DatabasePath}' was not found; skipping migrations.",
                databasePath);
            return;
        }

        var connection = (SqliteConnection)context.Database.GetDbConnection();

        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            if (await IndexExistsAsync(connection, EmailIndexName, cancellationToken))
            {
                logger.LogInformation(
                    "Migration '{Migration}' is already applied.",
                    UsersEmailMigration);
                return;
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }

        // Take a copy before rebuilding the Users table. The API is still
        // starting and is the only writer, so the file is consistent here.
        var backupPath = $"{databasePath}.bak-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        File.Copy(databasePath, backupPath, overwrite: false);

        logger.LogInformation(
            "Backed up SQLite database to '{BackupPath}'.",
            backupPath);

        var script = ReadMigrationScript();

        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = script;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            if (!await IndexExistsAsync(connection, EmailIndexName, cancellationToken) ||
                !await IndexExistsAsync(connection, DeletedAtIndexName, cancellationToken))
            {
                throw new InvalidOperationException(
                    "The Users email migration ran but the expected indexes are missing.");
            }

            if (await HasForeignKeyViolationsAsync(connection, cancellationToken))
            {
                throw new InvalidOperationException(
                    "The Users email migration left foreign key violations behind.");
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }

        logger.LogInformation(
            "Applied migration '{Migration}'.",
            UsersEmailMigration);
    }

    private static string ReadMigrationScript()
    {
        var assembly = typeof(DatabaseMigrationRunner).Assembly;

        using var stream = assembly.GetManifestResourceStream(UsersEmailMigration)
            ?? throw new InvalidOperationException(
                $"Embedded migration '{UsersEmailMigration}' was not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task<bool> IndexExistsAsync(
        SqliteConnection connection,
        string indexName,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT 1 FROM sqlite_master " +
            "WHERE type = 'index' AND name = $name LIMIT 1;";
        command.Parameters.AddWithValue("$name", indexName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    private static async Task<bool> HasForeignKeyViolationsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check;";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken);
    }
}
