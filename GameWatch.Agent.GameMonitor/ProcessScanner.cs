using System;
using System.Collections.Generic;
using System.Linq;
using GameWatch.Core;
using GameWatch.Core.Dto;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class ProcessScanner(AgentState state, ILogger<ProcessScanner> logger)
{
    public void Scan()
    {
        // Idx = GameId
        List<TrackingSessions.Auto> recentlyInactiveAutoGames = [];
        var availableProcs = ProcGatherer.GetDictOfAvailableProcesses();

        // 'remove inactives' step:
        // perform 'remove inactives' step:
        foreach (var pid in state.ActiveAutoGames.Keys)
        {
            if (availableProcs.ContainsKey(pid.V)) continue;

            if (!state.ActiveAutoGames.TryRemove(pid, out var session)) continue;
            state.ActiveAutoGamesPids.TryRemove(session.Game.Id, out _);
            recentlyInactiveAutoGames.Add(session);
        }

        // perform 'save recent inactives' step:
        foreach (var session in recentlyInactiveAutoGames)
        {
            var elapsed = (int)(DateTime.UtcNow - session.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed <= 0) continue;

            DbMng.GameLibrary.IncrementPlayTime(GameMode.Auto, session.Game.Id, elapsed);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("ℹ️ Auto Game Id={id} Name='{name}' has stopped. Elapsed={elapsed}s", session.Game.Id, session.Game.Name, elapsed);
        }

        // 'search match' step:
        // perform 'skip actives' step:
        var gameIdsToSkipCheckingWhenMonitoring = (from kvp in state.ActiveAutoGames
                                                   where availableProcs.ContainsKey(kvp.Key.V)
                                                   select kvp.Value.Game.Id).ToList();

        // 'perform actual search match' step:
        foreach (var game in state.LoadedAutoGames.Where(game => !gameIdsToSkipCheckingWhenMonitoring.Contains(game.Id)))
        {
            foreach (var (pid, proc) in availableProcs)
            {
                if (!RuleMatcher.IsMatch(proc, game)) continue;

                var newSession = new TrackingSessions.Auto { Game = game };
                var gamePid = new Pid(pid);
                state.ActiveAutoGames.TryAdd(gamePid, newSession);
                state.ActiveAutoGamesPids.TryAdd(game.Id, gamePid);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("ℹ️ Auto Game Id={id} Name='{name}' has started.", game.Id, game.Name);
            }
        }
    }
}