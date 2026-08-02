using System;
using GameWatch.Core.GameRecords;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Auto
{
    public required AutoGame Game { get; set; }
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}