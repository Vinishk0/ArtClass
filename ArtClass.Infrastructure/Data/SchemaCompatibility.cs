using Microsoft.EntityFrameworkCore;

namespace ArtClass.Infrastructure.Data;

/// <summary>
/// Idempotent schema patches for databases created before full EF migrations existed.
/// Uses PRAGMA table_info so repeated runs are safe on Android/iOS.
/// </summary>
internal static class SchemaCompatibility
{
    public static async Task EnsureAsync(ArtClassDbContext context, CancellationToken cancellationToken = default)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;
        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await AddColumnIfMissingAsync(connection,
                "StudyGroups", "IsRepeating", "INTEGER NOT NULL DEFAULT 1", cancellationToken);
            await AddColumnIfMissingAsync(connection,
                "StudyGroups", "Color", "TEXT NOT NULL DEFAULT '#C45C3E'", cancellationToken);
            await AddColumnIfMissingAsync(connection,
                "StudyGroups", "IsBiWeekly", "INTEGER NOT NULL DEFAULT 1", cancellationToken);

            await AddColumnIfMissingAsync(connection,
                "Lessons", "CycleWeek", "INTEGER NULL", cancellationToken);
            await AddColumnIfMissingAsync(connection,
                "Lessons", "SpecificDate", "TEXT NULL", cancellationToken);

            await EnsureIndexAsync(connection,
                "Lessons", "IX_Lessons_SpecificDate", "SpecificDate", cancellationToken);

            await EnsureStudentsTableAsync(connection, cancellationToken);
            await EnsureStudentStudyGroupsTableAsync(connection, cancellationToken);
            await EnsureScheduleSettingsTableAsync(connection, cancellationToken);
        }
        finally
        {
            if (shouldClose && connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task AddColumnIfMissingAsync(
        System.Data.Common.DbConnection connection,
        string table,
        string column,
        string columnDefinition,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, table, cancellationToken))
        {
            return;
        }

        if (await ColumnExistsAsync(connection, table, column, cancellationToken))
        {
            return;
        }

        await ExecuteAsync(connection,
            $"ALTER TABLE {table} ADD COLUMN {column} {columnDefinition};",
            cancellationToken);
    }

    private static async Task EnsureIndexAsync(
        System.Data.Common.DbConnection connection,
        string table,
        string indexName,
        string column,
        CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync(connection, table, cancellationToken))
        {
            return;
        }

        await ExecuteAsync(connection,
            $"CREATE INDEX IF NOT EXISTS {indexName} ON {table}({column});",
            cancellationToken);
    }

    private static async Task ExecuteAsync(
        System.Data.Common.DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        System.Data.Common.DbConnection connection,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = table;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(
        System.Data.Common.DbConnection connection,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(1);
            if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task EnsureStudentsTableAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(connection, "Students", cancellationToken))
        {
            return;
        }

        await ExecuteAsync(connection, """
            CREATE TABLE Students (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT NULL,
                Age INTEGER NULL
            );
            """, cancellationToken);
    }

    private static async Task EnsureStudentStudyGroupsTableAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(connection, "StudentStudyGroups", cancellationToken))
        {
            return;
        }

        await ExecuteAsync(connection, """
            CREATE TABLE StudentStudyGroups (
                StudentId INTEGER NOT NULL,
                StudyGroupId INTEGER NOT NULL,
                PRIMARY KEY (StudentId, StudyGroupId),
                FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
                FOREIGN KEY (StudyGroupId) REFERENCES StudyGroups(Id) ON DELETE CASCADE
            );
            """, cancellationToken);

        await ExecuteAsync(connection,
            "CREATE INDEX IF NOT EXISTS IX_StudentStudyGroups_StudyGroupId ON StudentStudyGroups(StudyGroupId);",
            cancellationToken);
    }

    private static async Task EnsureScheduleSettingsTableAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(connection, "ScheduleSettings", cancellationToken))
        {
            return;
        }

        await ExecuteAsync(connection, """
            CREATE TABLE ScheduleSettings (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                CycleStartDate TEXT NOT NULL
            );
            """, cancellationToken);
    }
}
