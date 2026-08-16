using System;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Auto : ITrackingSession
{
    public TableId TableId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public ProcPid Pid { get; init; }
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}