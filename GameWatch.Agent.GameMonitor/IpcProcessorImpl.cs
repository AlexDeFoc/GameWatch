using System;
using System.Linq;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Wrappers;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class IpcProcessorImpl(
    IHostApplicationLifetime lifetime,
    AgentState state,
    ILogger<IpcProcessorImpl> logger) : IpcProcessor.IpcProcessorBase
{
    public override Task<StatusIpcResponse> RemoveAutoGame(TableIdRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        if (state.ActiveAutoGamesPids.TryRemove(tableId, out var gamePid)
            && state.ActiveAutoGames.TryRemove(gamePid, out var gameSession)
            && logger.IsEnabled(LogLevel.Information))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed auto game with TableId='{id}' Name='{name}'", tableId.V, gameSession.Game.Name);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[INFO] Auto game with TableId={id} wasn't being tracked. Ignoring signal to remove auto game...", tableId.V);
        }

        state.RequestGameListRefresh();

        return Task.FromResult(new StatusIpcResponse { Ok = true });
    }

    public override Task<StatusIpcResponse> RemoveManualGame(TableIdRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        if (state.ActiveManualGames.TryRemove(tableId, out _)
            && logger.IsEnabled(LogLevel.Information))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed manual game with TableId='{id}'", tableId.V);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[INFO] Manual game with TableId={id} wasn't being tracked. Ignoring signal to remove manual game...", tableId.V);
        }

        state.RequestGameListRefresh();

        return Task.FromResult(new StatusIpcResponse { Ok = true });
    }

    public override Task<StatusIpcResponse> RefreshAutoGamesList(EmptyRequest request, ServerCallContext context)
    {
        state.RequestGameListRefresh();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refresh auto games signal received");

        return Task.FromResult(new StatusIpcResponse { Ok = true });
    }

    public override Task<ToggleManualGameResponse> ToggleManualGame(TableIdRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
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
                var queryGameNameResult = GameLibrary.Instance.QueryGameName(GameMode.Manual, tableId);

                if (queryGameNameResult is { Ok: true, GameName: not null })
                {
                    logger.LogInformation("[OK] Stopped manual game with TableId='{id}' Name='{name}', Elapsed={elapsed}s",
                                          tableId.V, queryGameNameResult.GameName, elapsed);
                }
                else
                {
                    logger.LogInformation("[WARN] Stopped manual game with TableId='{id}', Elapsed={elapsed}s. Though failed to get game name from db row...",
                                          tableId.V, elapsed);
                }
            }

            gameStarted = false;
        }
        else
        {
            var newSession = new TrackingSessions.Manual { Id = tableId };
            state.ActiveManualGames.TryAdd(tableId, newSession);

            if (logger.IsEnabled(LogLevel.Information))
            {
                var queryGameNameResult = GameLibrary.Instance.QueryGameName(GameMode.Manual, tableId);

                if (queryGameNameResult is { Ok: true, GameName: not null })
                {
                    logger.LogInformation("[OK] Started manual game with TableId='{id}' Name='{name}'",
                                          tableId, queryGameNameResult.GameName);
                }
                else
                {
                    logger.LogInformation("[WARN] Started manual game with TableId='{id}'. Though failed to get game name from db row...",
                                          tableId);
                }
            }

            gameStarted = true;
        }

        return Task.FromResult(new ToggleManualGameResponse { Ok = true, StartedGame = gameStarted });
    }

    public override Task<StatusAndMessageIpcResponse> ResetActiveManualGame(TableIdRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        var queryGameNameResult = GameLibrary.Instance.QueryGameName(GameMode.Manual, tableId);


        if (state.ActiveManualGames.TryGetValue(tableId, out var gameSession))
        {
            gameSession.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (!logger.IsEnabled(LogLevel.Information)) return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });

            if (queryGameNameResult is { Ok: true, GameName: not null })
            {
                logger.LogInformation("[OK] Manual game playtime with TableId='{id}' Name='{name}' got reset",
                                      tableId.V, queryGameNameResult.GameName);
            }
            else
            {
                logger.LogInformation("[OK] Manual game playtime with TableId='{id}' got reset. " +
                                      "Though failed to get game name from db row...", tableId.V);
            }
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            if (queryGameNameResult is { Ok: true, GameName: not null })
            {
                logger.LogWarning("[WARN] Manual game with TableId='{id}' Name='{name}' wasn't being tracked. " +
                                  "Ignoring signal to reset manual game playtime...", tableId.V, queryGameNameResult.GameName);
            }
            else
            {
                logger.LogWarning("[WARN] Manual game with TableId='{id}' wasn't being tracked. " +
                                  "Ignoring signal to reset manual game playtime...;" +
                                  "Additionally failed to get game name from db row...", tableId.V);
            }
        }

        return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
    }

    public override Task<StatusAndMessageIpcResponse> ResetActiveAutoGame(TableIdRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        if (state.ActiveAutoGames.TryGetValue(state.ActiveAutoGamesPids[tableId], out var gameSession))
        {
            gameSession.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Auto game playtime with TableId='{id}' Name='{name}' got reset",
                                      tableId.V, gameSession.Game.Name);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Auto Game with Id={id} wasn't being tracked. " +
                              "Ignoring signal to reset auto Game playtime...", tableId.V);
        }

        return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
    }

    public override Task<StatusAndMessageIpcResponse> EditAutoGame(EditGameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var matchingRulesChanged = request.MatchingRulesChanged;

        if (!matchingRulesChanged)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Auto game with TableId='{tableId}' did not have its matching rules changed. " +
                                      "Ignoring signal to refresh edited auto game...", tableId.V);

            return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
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
        var queryDisplayIdResult = GameLibrary.Instance.GetDisplayId(GameMode.Auto, tableId);

        if (!queryDisplayIdResult.Ok || queryDisplayIdResult.DisplayId is null)
            return Task.FromResult(new StatusAndMessageIpcResponse
            {
                Ok = false,
                Msg = queryDisplayIdResult.FailureReason
            });
        var displayId = queryDisplayIdResult.DisplayId.Value;

        var targetGame = state.LoadedAutoGames.FirstOrDefault(g => g.Id == displayId);

        if (targetGame is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("[FAIL] Cannot find auto game in db with TableId='{id}' during edit game signal processing. " +
                                "Considering game to have been deleted externally.", tableId.V);

            return Task.FromResult(new StatusAndMessageIpcResponse
            {
                Ok = false,
                Msg = $"[FAIL] Game with TableId='{tableId}' disappeared from database while processing in Game Monitor agent. " +
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

            return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
        }

        var gamePid = new Pid(targetGamePidValue.Value);

        // Create new tracking session
        var newSession = new TrackingSessions.Auto
        {
            Game = targetGame,
            LastTimeFlushedPlayTime = DateTime.UtcNow
        };

        state.ActiveAutoGames.TryAdd(gamePid, newSession);
        state.ActiveAutoGamesPids.TryAdd(tableId, gamePid);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refreshed matching rules for auto game with TableId={id} Name='{name}' (Detected as active).", tableId.V, targetGame.Name);

        return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
    }

    public override Task<StatusAndMessageIpcResponse> EvictAgent(EvictRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Eviction requested via gRPC. Triggering graceful shutdown...");

        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Give gRPC a moment to flush the response back to client
            lifetime.StopApplication();
        });

        return Task.FromResult(new StatusAndMessageIpcResponse { Ok = true });
    }
}