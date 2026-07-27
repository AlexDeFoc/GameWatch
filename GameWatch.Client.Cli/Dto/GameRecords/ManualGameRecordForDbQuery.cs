namespace GameWatch.Client.Cli.Dto.GameRecords;

public record ManualGameRecordForDbQuery(long TableId, long TablePositionIdx, string GameRecordTitle, long GameRecordPlayTime);