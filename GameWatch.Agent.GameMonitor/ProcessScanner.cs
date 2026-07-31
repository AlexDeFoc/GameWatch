using System;
using System.Collections.Generic;
using System.Linq;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class ProcessScanner(AgentState state, ILogger<ProcessScanner> logger)
{
    public void Scan()
    {
        var candidates = ProcessFinder.GetListOfAvailableProcesses();
        var currentPids = new HashSet<int>(candidates.Select(c => c.Pid));

        // 1. Detect process start
        foreach (var candidate in candidates.Where(c => !state.ActiveAutoGames.ContainsKey(c.Pid)))
        {
            foreach (var rule in state.LoadedAutoGames.Where(rule => RuleMatcher.IsMatch(candidate, rule)))
            {
                var isAlreadyTracked = state.ActiveAutoGames.Values.Any(session => session.GameId == rule.Idx);
                if (isAlreadyTracked)
                {
                    break;
                }

                var session = new TrackedSession
                              {
                                  GameId = rule.Idx,
                                  Mode = GameMode.Auto,
                                  LastFlushedUtc = DateTime.UtcNow
                              };

                if (state.ActiveAutoGames.TryAdd(candidate.Pid, session))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("[GAME STARTED] Auto Game '{Title}' (GameId: {GameId}, PID: {Pid})",
                                              rule.Title, rule.Idx, candidate.Pid);
                    }
                }

                break; // Process matched a rule, move to next process
            }
        }

        // 2. Detect process stop & flush remained time immediately
        var stoppedPids = state.ActiveAutoGames.Keys.Where(pid => !currentPids.Contains(pid)).ToList();

        foreach (var pid in stoppedPids)
        {
            if (!state.ActiveAutoGames.TryRemove(pid, out var session)) continue;

            var elapsedSeconds = (int)(DateTime.UtcNow - session.LastFlushedUtc).TotalSeconds;
            if (elapsedSeconds > 0)
            {
                DbFactory.GameLibrary.IncrementPlayTime(GameMode.Auto, session.GameId, elapsedSeconds);
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[GAME STOPPED] Auto Game (GameId: {GameId}, PID: {Pid}) - Flushed remaining +{Seconds}s",
                                      session.GameId, pid, elapsedSeconds);
            }
        }
    }
}