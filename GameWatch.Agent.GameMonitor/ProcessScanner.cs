using System;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class ProcessScanner(AgentState state, ILogger<ProcessScanner> logger)
{
    public void Scan()
    {
        var availableProcs = ProcGatherer.GetDictOfAvailableProcesses();
        var now = DateTime.UtcNow;

        // Remove inactives & flush playtime
        foreach (var (tableId, session) in state.ActiveAutoGames)
        {
            if (availableProcs.ContainsKey(session.Pid.V))
                continue;

            // Process termination: remove session
            if (!state.ActiveAutoGames.TryRemove(tableId, out var removedSession))
                continue;

            var elapsed = (long)(now - removedSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed <= 0)
            {
                GameLibrary.Instance.IncrementPlayTime(GameMode.Auto, removedSession.TableId, new ElapsedTime(elapsed));
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Auto game TableId='{id}' Name='{name}' has stopped. Elapsed={elapsed}s", removedSession.TableId.V, removedSession.GameName, elapsed);
        }

        // Search & match new games (Allows multiple TableIds to bind to the same PID)
        var loadedGames = state.LoadedAutoGames;
        for (var i = 0; i < loadedGames.Count; ++i)
        {
            var game = loadedGames[i];

            if (state.ActiveAutoGames.ContainsKey(game.TableId))
                continue;

            foreach (var (pid, proc) in availableProcs)
            {
                if (!RuleMatcher.IsMatch(proc, game))
                    continue;

                var newSession = new TrackingSessions.Auto { TableId = game.TableId, GameName = game.Name, Pid = new ProcPid(pid) };

                if (state.ActiveAutoGames.TryAdd(game.TableId, newSession))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("[INFO] Auto GameRecord TableId='{id}' Name='{name}' bound to PID='{pid}' (Game has started).", game.TableId.V, game.Name, pid);
                }

                break; // Match found for this game record, move to the next record
            }
        }
    }
}