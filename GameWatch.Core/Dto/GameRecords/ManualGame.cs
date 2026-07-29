// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace GameWatch.Core.Dto.GameRecords;

public sealed class ManualGame
{
    public int Idx { get; set; }
    public required string Title { get; set; }
    public int PlayTimeSeconds { get; set; }
}