namespace GameWatch.FileManager.FileSchemas.V2;

public sealed class AppSettingsTable
{
    public int Id { get; init; }
    public string AppLanguageTag { get; init; } = nameof(DataTypes.LanguageTag.en_US);
}