using System;
using GameWatch.Core.Dto;

namespace GameWatch.Agent.GameMonitor;

public sealed class TrackedSession
{
    public required int GameId { get; init; }
    public required GameMode Mode { get; init; }
    public DateTime LastFlushedUtc { get; set; } = DateTime.UtcNow;
}