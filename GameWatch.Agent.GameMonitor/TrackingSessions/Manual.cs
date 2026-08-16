using System;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public sealed class Manual
{
    public TableId TableId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public DateTime LastTimeFlushedPlayTime { get; set; } = DateTime.UtcNow;
}