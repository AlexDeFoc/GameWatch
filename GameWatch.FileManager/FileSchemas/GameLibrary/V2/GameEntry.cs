namespace GameWatch.FileManager.FileSchemas.GameLibrary.V2;

public class GameEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public long PlayTime { get; set; }
    public string FingerprintFullPath { get; set; } = string.Empty;
    public string FingerprintProcessName { get; set; } = string.Empty;
    public string FingerprintCommandLine { get; set; } = string.Empty;
    public string FingerprintProductName { get; set; } = string.Empty;
}