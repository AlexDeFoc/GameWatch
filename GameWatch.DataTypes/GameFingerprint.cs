namespace GameWatch.DataTypes;

public class GameFingerprint
{
    public string FullPath { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty; // e.g. "javaw" or "ForzaHorizon5"
    public string CommandLine { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}