// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace GameWatch.Client.Cli.Dto.GameRecords;

public sealed class AutoGameRecordSimplifiedForDbQuery
{
    public int TableId { get; init; }
    public int TablePositionIdx { get; init; }
    public string GameRecordTitle { get; init; } = null!;
    public int GameRecordPlayTime { get; init; }
}