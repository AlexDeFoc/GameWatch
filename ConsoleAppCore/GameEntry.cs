namespace GwConsoleAppCore;

public sealed class GameEntry
{
    public required string Title { get; set; }
    public required string TargetExecutablePath { get; set; }
    public GamePlaytime Playtime { get; set; } = new();
}