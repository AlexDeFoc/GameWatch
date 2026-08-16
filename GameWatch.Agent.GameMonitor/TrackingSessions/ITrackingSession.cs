using System;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor.TrackingSessions;

public interface ITrackingSession
{
    TableId TableId { get; }
    string GameName { get; }
    DateTime LastTimeFlushedPlayTime { get; set; }
}