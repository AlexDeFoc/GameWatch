namespace GameWatch.FileManager.FileSchemas.V2.GameLibrary;

public sealed class GameEntry
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public long PlayTime { get; init; }
    public string FingerprintFullPath { get; init; } = string.Empty;
    public string FingerprintProcessName { get; init; } = string.Empty;
    public string FingerprintCommandLine { get; init; } = string.Empty;
    public string FingerprintProductName { get; init; } = string.Empty;
}