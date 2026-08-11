using System;
using GameWatch.Core;
using GameWatch.Core.Dto;
using GameWatch.Core.Wrappers;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Manual
{
    public required GameId Id { get; init; }
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}