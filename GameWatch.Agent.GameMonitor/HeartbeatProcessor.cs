using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.TrackingSessions;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class HeartbeatProcessor(AgentState state, ILogger<HeartbeatProcessor> logger)
{
    public async Task FlushHeartbeats(CancellationToken cancellationToken, bool forceFlushAll = false)
    {
        var now = DateTime.UtcNow;

        await FlushSessionsAsync(state.ActiveAutoGames, GameMode.Auto, "Auto", now, forceFlushAll, cancellationToken);
        await FlushSessionsAsync(state.ActiveManualGames, GameMode.Manual, "Manual", now, forceFlushAll, cancellationToken);
    }

    private async Task FlushSessionsAsync<T>(
        ImmutableArray<T> gameSessions,
        GameMode gameMode,
        string modeLabel,
        DateTime utcNow,
        bool forceFlushAll,
        CancellationToken cancellationToken)
        where T : class, ITrackingSession
    {
        if (gameSessions.IsEmpty) return;

        Dictionary<TableId, ElapsedTime>? gamesElapsedToFlush = null;
        List<(TableId Id, string Name, ElapsedTime Elapsed)>? logBuffer = null;

        var threshold = Settings.GameMonitorAgent.Instance.CachedSettingPlayTimeFlushInterval;
        var isLogging = logger.IsEnabled(LogLevel.Information);

        foreach (var session in gameSessions)
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

            gamesElapsedToFlush ??= [];
            gamesElapsedToFlush[session.TableId] = elapsedTime;

            if (!isLogging) continue;

            logBuffer ??= [];
            logBuffer.Add((session.TableId, session.GameName, elapsedTime));
        }

        if (gamesElapsedToFlush is null) return;

        try
        {
            await GameLibrary.Instance.IncrementPlayTimeAsync(gameMode, gamesElapsedToFlush, cancellationToken);

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