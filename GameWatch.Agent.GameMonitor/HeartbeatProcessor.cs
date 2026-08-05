using System;
using System.Collections.Generic;
using GameWatch.Core;
using GameWatch.Core.Dto;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class HeartbeatProcessor(AgentState state, ILogger<HeartbeatProcessor> logger)
{
    /// <summary>
    /// Checks all active sessions. Flushes save increments for auto sessions,
    /// full accumulated time for manual sessions, or forces a full flush of all
    /// accumulated seconds during shutdown.
    /// </summary>
    public void FlushHeartbeats(bool forceFlushAll = false)
    {
        var now = DateTime.UtcNow;
        FlushAutoGames(state.ActiveAutoGames.Values, now, forceFlushAll);
        FlushManualGames(state.ActiveManualGames.Values, now, forceFlushAll);
    }

    private void FlushAutoGames(IEnumerable<TrackingSessions.Auto> gameSessions, DateTime utcNow, bool forceFlushAll)
    {
        var gamesToFlush = new Dictionary<GameId, ElapsedTime>();

        foreach (var s in gameSessions)
        {
            var elapsedSeconds = (int)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsedSeconds <= 0) continue;

            int secondsToFlush;

            if (forceFlushAll)
            {
                secondsToFlush = elapsedSeconds;
                s.LastTimeFlushedPlayTime = utcNow;
            }
            else
            {
                // Cap flush to exactly 60 seconds
                secondsToFlush = DbMng.Settings.GameMonitorAgentGamePlayTimeSaveThreshold;

                if (elapsedSeconds < secondsToFlush) continue;

                // Advance by 60s to keep any remaining seconds for the next tick
                s.LastTimeFlushedPlayTime = s.LastTimeFlushedPlayTime.AddSeconds(secondsToFlush);
            }

            gamesToFlush[s.Game.Id] = new ElapsedTime(secondsToFlush);
        }

        if (gamesToFlush.Count == 0) return;

        try
        {
            DbMng.GameLibrary.IncrementPlayTime(GameMode.Auto, gamesToFlush);

            if (!logger.IsEnabled(LogLevel.Information)) return;

            foreach (var (gameId, elapsed) in gamesToFlush)
            {
                logger.LogInformation("{timestamp} ℹ️ Activity: Auto Game Id={id} Elapsed={elapsed}s", utcNow.TimeOfDay, gameId, elapsed);
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "⛔ Failed to flush playtime for Active auto games");
        }
    }

    private void FlushManualGames(IEnumerable<TrackingSessions.Manual> gameSessions, DateTime utcNow, bool forceFlushAll)
    {
        var gamesToFlush = new Dictionary<GameId, ElapsedTime>();

        foreach (var s in gameSessions)
        {
            var elapsedSeconds = (int)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsedSeconds <= 0) continue;

            if (!forceFlushAll && elapsedSeconds < DbMng.Settings.GameMonitorAgentGamePlayTimeSaveThreshold) continue;

            // Manual games flush all accumulated seconds without capping
            gamesToFlush[s.Id] = new ElapsedTime(elapsedSeconds);

            // Advance by exact elapsed time to preserve fractional-second precision
            s.LastTimeFlushedPlayTime = s.LastTimeFlushedPlayTime.AddSeconds(elapsedSeconds);
        }

        if (gamesToFlush.Count == 0) return;

        try
        {
            DbMng.GameLibrary.IncrementPlayTime(GameMode.Manual, gamesToFlush);

            if (!logger.IsEnabled(LogLevel.Information)) return;

            foreach (var (gameId, elapsed) in gamesToFlush)
            {
                logger.LogInformation("{timestamp} ℹ️ Activity: Manual Game Id={id} Elapsed={elapsed}s", utcNow.TimeOfDay, gameId, elapsed);
            }
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "⛔ Failed to flush playtime for active manual games");
        }
    }
}