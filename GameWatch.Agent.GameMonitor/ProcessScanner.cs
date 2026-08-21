using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class ProcessScanner(AgentState state, ILogger<ProcessScanner> logger)
{
    public async Task ScanAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var seenPids = new HashSet<int>();
        var loadedGames = state.GetLoadedAutoGames();

        // Stream pass: Inspect procs one by one as they are enumerated from the OS
        await foreach (var proc in ProcGatherer.StreamAvailableProcessesAsync(cancellationToken))
        {
            seenPids.Add(proc.Pid);

            // Match against loaded games that aren't currently active
            foreach (var game in loadedGames)
            {
                if (state.TryGetActiveAutoGame(game.TableId))
                    continue;

                if (!RuleMatcher.IsMatch(proc, game))
                    continue;

                var newSession = new TrackingSessions.Auto
                {
                    TableId = game.TableId,
                    GameName = game.Name,
                    Pid = new ProcPid(proc.Pid),
                    LastTimeFlushedPlayTime = now
                };

                if (!state.AddActiveAutoGame(newSession)) continue;
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("[INFO] Auto GameRecord TableId='{id}' Name='{name}' bound to PID='{pid}' " +
                                          "(Game has started).", game.TableId.V, game.Name, proc.Pid);
                }
            }
        }

        // Cleanup pass: Check active sessions against the seen PIDs
        foreach (var session in state.ActiveAutoGames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (seenPids.Contains(session.Pid.V))
                continue;

            // Process no longer exists in seenPids -> Terminated
            if (!state.RemoveActiveAutoGame(session.TableId, out var removedSession))
                continue;

            var elapsed = (long)(now - removedSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
            {
                try
                {
                    await GameLibrary.Instance.IncrementPlayTimeAsync(GameMode.Auto, removedSession.TableId, new ElapsedTime(elapsed), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Restore session so unsaved elapsed time isn't lost
                    state.AddActiveAutoGame(removedSession);
                    throw;
                }
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Auto game TableId='{id}' Name='{name}' has stopped. Elapsed={elapsed}s",
                                      removedSession.TableId.V, removedSession.GameName, elapsed);
        }
    }
}