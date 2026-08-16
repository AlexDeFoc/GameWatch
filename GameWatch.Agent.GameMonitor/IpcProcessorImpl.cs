using System;
using System.Linq;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class IpcProcessorImpl(
    IHostApplicationLifetime lifetime,
    AgentState state,
    ILogger<IpcProcessorImpl> logger) : IpcProcessor.IpcProcessorBase
{
    public override Task<StatusResponse> RemoveAutoGame(GameIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveAutoGamesPids.TryRemove(tableId, out var gamePid)
            && state.ActiveAutoGames.TryRemove(gamePid, out _)
            && logger.IsEnabled(LogLevel.Information))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed auto game with TableId='{id}' Name='{name}'", tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[INFO] Auto game with TableId='{id}' Name='{name}' wasn't being tracked. Ignoring signal to remove auto game...", tableId.V, gameName);
        }

        state.RequestGameListRefresh();

        return Task.FromResult(new StatusResponse { Ok = true });
    }

    public override Task<StatusResponse> RemoveManualGame(GameIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveManualGames.TryRemove(tableId, out _)
            && logger.IsEnabled(LogLevel.Information))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed manual game with TableId='{id}' Name='{name}'", tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[INFO] Manual game with TableId='{id}' Name='{name}' wasn't being tracked. Ignoring signal to remove manual game...", tableId.V, gameName);
        }

        state.RequestGameListRefresh();

        return Task.FromResult(new StatusResponse { Ok = true });
    }

    public override Task<StatusResponse> RefreshAutoGamesList(EmptyRequest request, ServerCallContext context)
    {
        state.RequestGameListRefresh();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refresh auto games signal received");

        return Task.FromResult(new StatusResponse { Ok = true });
    }

    public override Task<ToggleManualGameResponse> ToggleManualGame(GameIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;
        bool gameStarted;

        if (state.ActiveManualGames.TryRemove(tableId, out var gameSession))
        {
            var elapsed = (long)(DateTime.UtcNow - gameSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
            {
                GameLibrary.Instance.IncrementPlayTime(GameMode.Manual, tableId, new ElapsedTime(elapsed));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[OK] Stopped manual game with TableId='{id}' Name='{name}', Elapsed={elapsed}s",
                                      tableId.V, gameName, elapsed);
            }

            gameStarted = false;
        }
        else
        {
            var newSession = new TrackingSessions.Manual { TableId = tableId, GameName = gameName };
            state.ActiveManualGames.TryAdd(tableId, newSession);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[OK] Started manual game with TableId='{id}' Name='{name}'",
                                      tableId, gameName);
            }

            gameStarted = true;
        }

        return Task.FromResult(new ToggleManualGameResponse { Ok = true, StartedGame = gameStarted });
    }

    public override Task<StatusAndMessageResponse> ResetActiveManualGame(GameIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveManualGames.TryGetValue(tableId, out var gameSession))
        {
            gameSession.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (!logger.IsEnabled(LogLevel.Information)) return Task.FromResult(new StatusAndMessageResponse { Ok = true });

            logger.LogInformation("[OK] Manual game playtime with TableId='{id}' Name='{name}' got reset",
                                  tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Manual game with TableId='{id}' Name='{name}' wasn't being tracked. " +
                              "Ignoring signal to reset manual game playtime...", tableId.V, gameName);
        }

        return Task.FromResult(new StatusAndMessageResponse { Ok = true });
    }

    public override Task<StatusAndMessageResponse> ResetActiveAutoGame(GameIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveAutoGames.TryGetValue(state.ActiveAutoGamesPids[tableId], out var gameSession))
        {
            gameSession.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Auto game playtime with TableId='{id}' Name='{name}' got reset",
                                      tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Auto GameRecord with Id='{id}' Name='{name}' wasn't being tracked. " +
                              "Ignoring signal to reset auto GameRecord playtime...", tableId.V, gameName);
        }

        return Task.FromResult(new StatusAndMessageResponse { Ok = true });
    }

    public override Task<StatusAndMessageResponse> EditAutoGame(EditGameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;
        var matchingRulesChanged = request.MatchingRulesChanged;

        if (!matchingRulesChanged)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Auto game with TableId='{tableId}' Name='{name}' did not have its matching rules changed. " +
                                      "Ignoring signal to refresh edited auto game...", tableId.V, gameName);

            return Task.FromResult(new StatusAndMessageResponse { Ok = true });
        }

        // Saved until now elapsed time
        if (state.ActiveAutoGamesPids.TryRemove(tableId, out var currentGamePid)
            && state.ActiveAutoGames.TryRemove(currentGamePid, out var currentGameSession))
        {
            var elapsed = (long)(DateTime.UtcNow - currentGameSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
                GameLibrary.Instance.IncrementPlayTime(GameMode.Auto, tableId, new ElapsedTime(elapsed));
        }

        // Refresh auto games list for next heartbeat
        state.LoadedAutoGames.ReplaceAll(GameLibrary.Instance.GetAutoGames());

        // Grab target from loaded games
        var targetGame = state.LoadedAutoGames.FirstOrDefault(g => g.TableId == tableId);

        if (targetGame is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("[FAIL] Cannot find auto game in db with TableId='{id}' during edit game signal processing. " +
                                "Considering game to have been deleted externally.", tableId.V);

            return Task.FromResult(new StatusAndMessageResponse
            {
                Ok = false,
                Msg = $"[FAIL] GameRecord with TableId='{tableId}' disappeared from database " +
                      $"while processing in GameRecord Monitor agent. " +
                      $"Abandoning finding game with new matching rules..."
            });
        }

        // Find game with new matching rules
        var procs = ProcGatherer.GetListOfAvailableProcesses();
        var targetGamePidValue = procs.FirstOrDefault(proc => RuleMatcher.IsMatch(proc, targetGame))?.Pid;

        if (targetGamePidValue is null)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Refreshed matching rules for auto game with TableId={id} Name='{name}' (Detected as inactive).", tableId.V, targetGame.Name);

            return Task.FromResult(new StatusAndMessageResponse { Ok = true });
        }

        var gamePid = new Pid(targetGamePidValue.Value);

        // Create new tracking session
        var newSession = new TrackingSessions.Auto
        {
            TableId = targetGame.TableId,
            GameName = targetGame.Name,
            LastTimeFlushedPlayTime = DateTime.UtcNow
        };

        state.ActiveAutoGames.TryAdd(gamePid, newSession);
        state.ActiveAutoGamesPids.TryAdd(tableId, gamePid);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refreshed matching rules for auto game with TableId='{id}' Name='{name}' (Detected as active).", tableId.V, targetGame.Name);

        return Task.FromResult(new StatusAndMessageResponse { Ok = true });
    }

    public override Task<StatusResponse> EvictOldInstance(EmptyRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Eviction requested via gRPC. Triggering graceful shutdown...");

        _ = Task.Run(async () =>
        {
            await Task.Delay(5000); // Give gRPC a moment to flush the response back to client
            lifetime.StopApplication();
        });

        return Task.FromResult(new StatusResponse { Ok = true });
    }
}