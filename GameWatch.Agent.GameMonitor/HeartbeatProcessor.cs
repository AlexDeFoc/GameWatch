using System;
using System.Collections.Generic;
using GameWatch.Core;
using GameWatch.Core.Dto;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class HeartbeatProcessor(AgentState state, ILogger<HeartbeatProcessor> logger)
{
    /// <summary>
    /// Checks all active sessions. Flushes 60s increments for sessions that reached the threshold,
    /// or forces a full flush of all accumulated seconds during shutdown.
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
            var elapsed = (int)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            switch (elapsed)
            {
                case <= 0:
                case < 60 when !forceFlushAll:
                    continue;
                default:
                    // Flush if forced, or if at least 60 seconds have passed
                    gamesToFlush[s.Game.Id] = new ElapsedTime(elapsed);
                    s.LastTimeFlushedPlayTime = utcNow;
                    break;
            }
        }

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
            var elapsed = (int)(utcNow - s.LastTimeFlushedPlayTime).TotalSeconds;
            switch (elapsed)
            {
                case <= 0:
                case < 60 when !forceFlushAll:
                    continue;
                default:
                    // Flush if forced, or if at least 60 seconds have passed
                    gamesToFlush[s.Id] = new ElapsedTime(elapsed);
                    s.LastTimeFlushedPlayTime = utcNow;
                    break;
            }
        }

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