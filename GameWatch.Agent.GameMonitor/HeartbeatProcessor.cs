using System;
using System.Collections.Generic;
using GameWatch.Agent.GameMonitor.TrackingSessions;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class HeartbeatProcessor(AgentState state, ILogger<HeartbeatProcessor> logger)
{
    public void FlushHeartbeats(bool forceFlushAll = false)
    {
        var now = DateTime.UtcNow;
        FlushSessions(state.ActiveAutoGames, GameMode.Auto, "Auto", now, forceFlushAll);
        FlushSessions(state.ActiveManualGames, GameMode.Manual, "Manual", now, forceFlushAll);
    }

    private void FlushSessions<T>(ICollection<KeyValuePair<TableId, T>> gameSessions, GameMode gameMode, string modeLabel, DateTime utcNow, bool forceFlushAll)
        where T : class, ITrackingSession
    {
        Dictionary<TableId, ElapsedTime>? gamesElapsedToFlush = null;

        // Parallel tracking array/list for logging ONLY if logging is enabled (Lazy)
        List<(TableId Id, string Name, ElapsedTime Elapsed)>? logBuffer = null;

        var threshold = Settings.Instance.GameMonitorAgentGamePlayTimeSaveThreshold;
        var isLogging = logger.IsEnabled(LogLevel.Information);

        // Enumerating KeyValuePair directly over ConcurrentDictionary avoids .Values allocation
        foreach (var (tableId, session) in gameSessions)
        {
            long secondsToFlush;
            lock (session)
            {
                var elapsedSeconds = (long)(utcNow - session.LastTimeFlushedPlayTime).TotalSeconds;
                if (elapsedSeconds <= 0) continue;

                if (forceFlushAll)
                {
                    secondsToFlush = elapsedSeconds;
                    session.LastTimeFlushedPlayTime = utcNow;
                }
                else
                {
                    secondsToFlush = threshold;
                    if (elapsedSeconds < secondsToFlush) continue;

                    session.LastTimeFlushedPlayTime = session.LastTimeFlushedPlayTime.AddSeconds(secondsToFlush);
                }
            }

            var elapsedTime = new ElapsedTime(secondsToFlush);

            // Lazy init primary payload
            gamesElapsedToFlush ??= [];
            gamesElapsedToFlush[tableId] = elapsedTime;

            // Only track names if LogLevel.Information is enabled
            if (!isLogging) continue;

            logBuffer ??= [];
            logBuffer.Add((tableId, session.GameName, elapsedTime));
        }

        if (gamesElapsedToFlush is null) return;

        try
        {
            GameLibrary.Instance.IncrementPlayTime(gameMode, gamesElapsedToFlush);

            if (logBuffer is null || !logger.IsEnabled(LogLevel.Information)) return;

            foreach (var (id, name, elapsed) in logBuffer)
            {
                logger.LogInformation("[INFO] Activity: {mode} game with TableId='{id}' Name='{name}', Elapsed={elapsed}s",
                                      modeLabel, id.V, name, elapsed.V);
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "[FAIL] Failed to flush playtime for active {mode} games", modeLabel);
        }
    }
}