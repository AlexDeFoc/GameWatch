using System;
using GameWatch.Core.Dto;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Manual
{
    public required GameId Id { get; init; }
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}