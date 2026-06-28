namespace GameWatch.FileManager.FileSchemas.V2.AppSettings;

public sealed class Settings
{
    public int Id { get; init; }
    public string AppLanguageTag { get; init; } = nameof(DataTypes.LanguageTag.en_US);
}