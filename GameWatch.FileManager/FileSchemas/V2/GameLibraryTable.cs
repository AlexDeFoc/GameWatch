using GameWatch.DataTypes;

namespace GameWatch.FileManager.FileSchemas.V2;

public sealed class GameLibraryTable
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Mode { get; init; } = nameof(GameMode.Manual);
    public long PlayTime { get; init; }
    public bool IsActive { get; init; } = false;
    public string FingerprintFullPath { get; init; } = string.Empty;
    public string FingerprintProcessName { get; init; } = string.Empty;
    public string FingerprintCommandLine { get; init; } = string.Empty;
    public string FingerprintProductName { get; init; } = string.Empty;
}