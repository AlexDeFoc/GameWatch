using System;
using System.Collections.Generic;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
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
        FlushSessionGroup(state.ActiveAutoGames.Values, GameMode.Auto, now, forceFlushAll);
        FlushSessionGroup(state.ActiveManualGames.Values, GameMode.Manual, now, forceFlushAll);
    }

    private void FlushSessionGroup(IEnumerable<TrackedSession> sessions, GameMode mode, DateTime now, bool forceFlushAll)
    {
        var gamesToFlush = new Dictionary<int, int>(); // GameId -> Seconds to add

        foreach (var session in sessions)
        {
            var elapsedSeconds = (int)(now - session.LastFlushedUtc).TotalSeconds;

            if (forceFlushAll)
            {
                if (elapsedSeconds > 0)
                {
                    gamesToFlush[session.GameId] = gamesToFlush.GetValueOrDefault(session.GameId) + elapsedSeconds;
                    session.LastFlushedUtc = now;
                }
            }
            else if (elapsedSeconds >= 60)
            {
                // Flush 60-second chunks
                var chunks = elapsedSeconds / 60;
                var secondsToFlush = chunks * 60;

                gamesToFlush[session.GameId] = gamesToFlush.GetValueOrDefault(session.GameId) + secondsToFlush;
                session.LastFlushedUtc = session.LastFlushedUtc.AddSeconds(secondsToFlush);
            }
        }

        foreach (var (gameId, seconds) in gamesToFlush)
        {
            try
            {
                DbFactory.GameLibrary.IncrementPlayTime(mode, [gameId], seconds);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[HEARTBEAT] Updated {Mode} Game (GameId: {GameId}) (+{Seconds}s)", mode, gameId, seconds);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "[HEARTBEAT ERROR] Failed to flush playtime for {Mode} Game Id {GameId}", mode, gameId);
            }
        }
    }
}