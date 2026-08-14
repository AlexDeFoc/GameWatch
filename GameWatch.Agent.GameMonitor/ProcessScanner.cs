using System;
using System.Collections.Generic;
using System.Linq;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Wrappers;
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

            var queryTableIdResult = GameLibrary.Instance.GetTableId(GameMode.Auto, session.Game.Id);

            if (!queryTableIdResult.Ok || queryTableIdResult.TableId is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("[ERROR] Cannot find active game with in-memory DisplayId='{id}' Name='{name}' " +
                                    "to be able to process game in step 'remove inactives'. " +
                                    "Its possible the game was removed and the agent didn't receive the signal. " +
                                    "Skipping game...", session.Game.Id, session.Game.Name);

                continue;
            }

            var tableId = queryTableIdResult.TableId.Value;

            state.ActiveAutoGamesPids.TryRemove(tableId, out _);
            recentlyInactiveAutoGames.Add(session);
        }

        // perform 'save recent inactives' step:
        foreach (var session in recentlyInactiveAutoGames)
        {
            var elapsed = (long)(DateTime.UtcNow - session.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed <= 0) continue;

            var queryTableIdResult = GameLibrary.Instance.GetTableId(GameMode.Auto, session.Game.Id);

            if (!queryTableIdResult.Ok || queryTableIdResult.TableId is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("[ERROR] Cannot find active game with in-memory DisplayId='{id}' Name='{name}' " +
                                    "to be able to process game in step 'save recent inactives'. " +
                                    "Its possible the game was removed and the agent didn't receive the signal. " +
                                    "Skipping game...", session.Game.Id, session.Game.Name);

                continue;
            }

            var tableId = queryTableIdResult.TableId.Value;

            GameLibrary.Instance.IncrementPlayTime(GameMode.Auto, tableId, new ElapsedTime(elapsed));

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Auto Game TableId='{id}' Name='{name}' has stopped. Elapsed={elapsed}s", tableId, session.Game.Name, elapsed);
        }

        // 'search match' step:
        // 'create skip set' step:
        var activeGameIds = state.ActiveAutoGames.Values
                                 .Select(s => s.Game.Id)
                                 .ToHashSet();

        // 'perform actual search match' step:
        foreach (var game in state.LoadedAutoGames)
        {
            if (activeGameIds.Contains(game.Id))
                continue;

            var queryTableIdResult = GameLibrary.Instance.GetTableId(GameMode.Auto, game.Id);

            if (!queryTableIdResult.Ok || queryTableIdResult.TableId is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("[ERROR] Cannot find active game with in-memory DisplayId='{id}' Name='{name}' " +
                                    "to be able to process game in step 'perform actual search match'. " +
                                    "Its possible the game was removed and the agent didn't receive the signal. " +
                                    "Skipping game...", game.Id, game.Name);

                continue;
            }

            var tableId = queryTableIdResult.TableId.Value;

            foreach (var (pid, proc) in availableProcs)
            {
                if (!RuleMatcher.IsMatch(proc, game)) continue;

                var newSession = new TrackingSessions.Auto { Game = game };
                var gamePid = new Pid(pid);

                state.ActiveAutoGames.TryAdd(gamePid, newSession);
                state.ActiveAutoGamesPids.TryAdd(tableId, gamePid);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[INFO] Auto Game TableId='{id}' Name='{name}' has started.", tableId, game.Name);

                break;
            }
        }
    }
}