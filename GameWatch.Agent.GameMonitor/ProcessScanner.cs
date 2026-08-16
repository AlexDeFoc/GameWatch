using System;
using System.Collections.Generic;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class ProcessScanner(AgentState state, ILogger<ProcessScanner> logger)
{
    public void Scan()
    {
        // Idx = Id
        List<TrackingSessions.Auto> recentlyInactiveAutoGames = [];
        var availableProcs = ProcGatherer.GetDictOfAvailableProcesses();

        // 'remove inactives' step:
        // perform 'remove inactives' step:
        foreach (var pid in state.ActiveAutoGames.Keys)
        {
            if (availableProcs.ContainsKey(pid.V)) continue;

            if (!state.ActiveAutoGames.TryRemove(pid, out var session)) continue;

            state.ActiveAutoGamesPids.TryRemove(session.TableId, out _);
            recentlyInactiveAutoGames.Add(session);
        }

        // perform 'save recent inactives' step:
        foreach (var session in recentlyInactiveAutoGames)
        {
            var elapsed = (long)(DateTime.UtcNow - session.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed <= 0) continue;

            var gameName = session.GameName;
            var tableId = session.TableId;

            GameLibrary.Instance.IncrementPlayTime(GameMode.Auto, tableId, new ElapsedTime(elapsed));

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Auto GameRecord TableId='{id}' Name='{name}' has stopped. Elapsed={elapsed}s", tableId.V, gameName, elapsed);
        }

        // 'search match' step:
        // 'perform actual search match' step:
        foreach (var game in state.LoadedAutoGames)
        {
            // 'create skip set' step:
            if (state.ActiveAutoGamesPids.TryGetValue(game.TableId, out _))
                continue;

            foreach (var (pid, proc) in availableProcs)
            {
                if (!RuleMatcher.IsMatch(proc, game)) continue;

                var newSession = new TrackingSessions.Auto { TableId = game.TableId, GameName = game.Name };
                var gamePid = new Pid(pid);

                state.ActiveAutoGames.TryAdd(gamePid, newSession);
                state.ActiveAutoGamesPids.TryAdd(game.TableId, gamePid);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[INFO] Auto GameRecord TableId='{id}' Name='{name}' has started.", game.TableId.V, game.Name);

                break;
            }
        }
    }
}