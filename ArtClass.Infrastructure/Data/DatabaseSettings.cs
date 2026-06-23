namespace ArtClass.Infrastructure.Data;

public sealed class DatabaseSettings
{
    public required string DatabasePath { get; init; }

    public string Directory => Path.GetDirectoryName(DatabasePath) ?? string.Empty;

    public string FileName => Path.GetFileName(DatabasePath);
}
