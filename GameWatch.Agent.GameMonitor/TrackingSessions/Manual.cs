using System;
using GameWatch.Core.Dto;
using GameWatch.Core.Wrappers;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Manual
{
    public TableId Id { get; set; }
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}