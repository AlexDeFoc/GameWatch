// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace GameWatch.Client.Cli.Dto.GameRecords;

public sealed class ManualGameRecord
{
    public required string Title { get; init; }
    public required int PlayTime { get; init; }
}