using System;
using System.Collections.Generic;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class HeartbeatProcessor(AgentState state, ILogger<HeartbeatProcessor> logger)
{
    public void FlushHeartbeats(bool forceFlushAll = false)
    {
        var now = DateTime.UtcNow;
        FlushAutoGames(state.ActiveAutoGames.Values, now, forceFlushAll);
        FlushManualGames(state.ActiveManualGames.Values, now, forceFlushAll);
    }

    private void FlushAutoGames(IEnumerable<TrackingSessions.Auto> gameSessions, DateTime utcNow, bool forceFlushAll)
    {
        var gamesElapsedToFlush = new Dictionary<TableId, ElapsedTime>();
        var gameNames = new Dictionary<TableId, string>();

        foreach (var s in gameSessions)
        {
            var elapsedSeconds = (long)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsedSeconds <= 0) continue;

            long secondsToFlush;

            if (forceFlushAll)
            {
                secondsToFlush = elapsedSeconds;
                s.LastTimeFlushedPlayTime = utcNow;
            }
            else
            {
                // Cap flush to exactly 60 seconds
                secondsToFlush = Settings.Instance.GameMonitorAgentGamePlayTimeSaveThreshold;

                if (elapsedSeconds < secondsToFlush) continue;

                // Advance by 60s to keep any remaining seconds for the next tick
                s.LastTimeFlushedPlayTime = s.LastTimeFlushedPlayTime.AddSeconds(secondsToFlush);
            }

            gamesElapsedToFlush[s.TableId] = new ElapsedTime(secondsToFlush);
            gameNames[s.TableId] = s.GameName;
        }

        if (gamesElapsedToFlush.Count == 0) return;

        try
        {
            GameLibrary.Instance.IncrementPlayTime(GameMode.Manual, gamesElapsedToFlush);

            if (!logger.IsEnabled(LogLevel.Information)) return;

            foreach (var (tableId, elapsed) in gamesElapsedToFlush)
            {
                logger.LogInformation("[INFO] Activity: Auto game with TableId='{id}' Name='{name}', Elapsed={elapsed}s", tableId.V, gameNames[tableId], elapsed.V);
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "[FAIL] Failed to flush playtime for active auto games");
        }
    }

    private void FlushManualGames(IEnumerable<TrackingSessions.Manual> gameSessions, DateTime utcNow, bool forceFlushAll)
    {
        var gamesElapsedToFlush = new Dictionary<TableId, ElapsedTime>();
        var gameNames = new Dictionary<TableId, string>();

        foreach (var s in gameSessions)
        {
            var elapsedSeconds = (long)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsedSeconds <= 0) continue;

            long secondsToFlush;

            if (forceFlushAll)
            {
                secondsToFlush = elapsedSeconds;
                s.LastTimeFlushedPlayTime = utcNow;
            }
            else
            {
                // Cap flush to exactly 60 seconds
                secondsToFlush = Settings.Instance.GameMonitorAgentGamePlayTimeSaveThreshold;
                if (elapsedSeconds < secondsToFlush) continue;

                // Advance by 60s to keep any remaining seconds for the next tick
                s.LastTimeFlushedPlayTime = s.LastTimeFlushedPlayTime.AddSeconds(secondsToFlush);
            }

            gamesElapsedToFlush[s.TableId] = new ElapsedTime(secondsToFlush);
            gameNames[s.TableId] = s.GameName;
        }

        if (gamesElapsedToFlush.Count is 0) return;

        try
        {
            GameLibrary.Instance.IncrementPlayTime(GameMode.Manual, gamesElapsedToFlush);

            if (!logger.IsEnabled(LogLevel.Information)) return;

            foreach (var (tableId, elapsed) in gamesElapsedToFlush)
            {
                logger.LogInformation("[INFO] Activity: Manual game with TableId='{id}' Name='{name}', Elapsed={elapsed}s", tableId.V, gameNames[tableId], elapsed.V);
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "[FAIL] Failed to flush playtime for active manual games");
        }
    }
}