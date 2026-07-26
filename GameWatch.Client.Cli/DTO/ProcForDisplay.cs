namespace GameWatch.Client.Cli.DTO;

public record ProcForDisplay(
    long Pid,
    string WindowTitle,
    string ProcName,
    string FilePath);