using System;
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
    private static readonly StatusAndMessageResponse OkStatusAndMessageResponse = new() { Ok = true };
    private static readonly StatusResponse OkStatusResponse = new() { Ok = true };

    private static readonly Task<StatusResponse> OkStatusTask =
        Task.FromResult(new StatusResponse { Ok = true });

    private static readonly Task<StatusAndMessageResponse> OkStatusAndMessageTask =
        Task.FromResult(new StatusAndMessageResponse { Ok = true });

    public override Task<StatusResponse> RemoveAutoGame(TableIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        if (state.ActiveAutoGames.TryRemove(tableId, out var s)
            && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("[OK] Removed auto game with TableId='{id}' Name='{name}'", s.TableId.V, s.GameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            var gameName = request.GameName;
            logger.LogWarning("[INFO] Auto game with TableId='{id}' Name='{name}' wasn't being tracked. Ignoring signal to remove auto game...", tableId.V, gameName);
        }

        state.RemoveAutoGame(tableId);

        return OkStatusTask;
    }

    public override Task<StatusResponse> RemoveManualGame(TableIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveManualGames.TryRemove(tableId, out _)
            && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("[OK] Removed manual game with TableId='{id}' Name='{name}'", tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[INFO] Manual game with TableId='{id}' Name='{name}' wasn't being tracked. Ignoring signal to remove manual game...", tableId.V, gameName);
        }

        return OkStatusTask;
    }

    public override Task<StatusAndMessageResponse> StopTrackingAllGames(StopTrackingAllGamesRequest request, ServerCallContext context)
    {
        if (request.StopTrackingAutoGames)
        {
            state.ActiveAutoGames.Clear();
            state.RemoveAllAutoGames();
        }

        if (request.StopTrackingManualGames)
        {
            state.ActiveManualGames.Clear();
        }

        return OkStatusAndMessageTask;
    }

    public override Task<StatusAndMessageResponse> TrackNewlyAddedAutoGame(AutoGameDtoRequest game, ServerCallContext context)
    {
        state.AddAutoGame(new AutoGameRecord
        {
            TableId = new TableId(game.TableId),
            Name = game.GameName,
            PlayTimeSec = new ElapsedTime(game.PlayTimeSec),
            WindowTitle = string.IsNullOrEmpty(game.WindowTitle) ? null : game.WindowTitle,
            WindowRule = string.IsNullOrEmpty(game.WindowRule) ? null : game.WindowRule,
            FilePath = string.IsNullOrEmpty(game.FilePath) ? null : game.FilePath,
            PathRule = string.IsNullOrEmpty(game.PathRule) ? null : game.PathRule
        });

        return OkStatusAndMessageTask;
    }

    public override async Task<ToggleManualGameResponse> ToggleManualGame(TableIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;
        bool gameStarted;

        if (state.ActiveManualGames.TryRemove(tableId, out var gameSession))
        {
            var elapsed = (long)(DateTime.UtcNow - gameSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
            {
                await GameLibrary.Instance.IncrementPlayTimeAsync(GameMode.Manual, tableId, new ElapsedTime(elapsed), context.CancellationToken);
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

        return new ToggleManualGameResponse { Ok = true, StartedGame = gameStarted };
    }

    public override Task<StatusAndMessageResponse> ResetActiveAutoGame(TableIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);

        if (state.ActiveAutoGames.TryGetValue(tableId, out var s))
        {
            lock (s)
            {
                s.LastTimeFlushedPlayTime = DateTime.UtcNow;
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Auto game playtime with TableId='{id}' Name='{name}' got reset",
                                      s.TableId.V, s.GameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            var gameName = request.GameName;
            logger.LogWarning("[WARN] Auto GameRecord with Id='{id}' Name='{name}' wasn't being tracked. " +
                              "Ignoring signal to reset auto GameRecord playtime...", tableId.V, gameName);
        }

        return OkStatusAndMessageTask;
    }

    public override Task<StatusAndMessageResponse> ResetActiveManualGame(TableIdAndNameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;

        if (state.ActiveManualGames.TryGetValue(tableId, out var gameSession))
        {
            lock (gameSession)
            {
                gameSession.LastTimeFlushedPlayTime = DateTime.UtcNow;
            }

            if (!logger.IsEnabled(LogLevel.Information)) return OkStatusAndMessageTask;

            logger.LogInformation("[OK] Manual game playtime with TableId='{id}' Name='{name}' got reset",
                                  tableId.V, gameName);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Manual game with TableId='{id}' Name='{name}' wasn't being tracked. " +
                              "Ignoring signal to reset manual game playtime...", tableId.V, gameName);
        }

        return OkStatusAndMessageTask;
    }

    public override async Task<StatusAndMessageResponse> EditAutoGame(EditGameRequest request, ServerCallContext context)
    {
        var tableId = new TableId(request.TableId);
        var gameName = request.GameName;
        var matchingRulesChanged = request.MatchingRulesChanged;

        if (!matchingRulesChanged)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Auto game with TableId='{tableId}' Name='{name}' did not have its matching rules changed. " +
                                      "Ignoring signal to refresh edited auto game...", tableId.V, gameName);

            return OkStatusAndMessageResponse;
        }

        // Saved until now elapsed time
        if (state.ActiveAutoGames.TryRemove(tableId, out var currentGameSession))
        {
            var elapsed = (long)(DateTime.UtcNow - currentGameSession.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
            {
                try
                {
                    await GameLibrary.Instance.IncrementPlayTimeAsync(GameMode.Auto, tableId, new ElapsedTime(elapsed), context.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    state.ActiveAutoGames.TryAdd(tableId, currentGameSession);
                    throw;
                }
            }
        }

        // Refresh auto games list for next heartbeat via snapshot

        var freshGames = await GameLibrary.Instance.GetAutoGamesAsync(context.CancellationToken);
        state.ReplaceAllAutoGames(freshGames);

        // Grab target from loaded games from fresh snapshot
        AutoGameRecord? targetGame = null;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var g in freshGames)
        {
            if (g.TableId != tableId) continue;
            targetGame = g;
            break;
        }

        if (targetGame is null)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("[FAIL] Cannot find auto game in db with TableId='{id}' during edit game signal processing. " +
                                "Considering game to have been deleted externally.", tableId.V);

            return new StatusAndMessageResponse
            {
                Ok = false,
                Msg = $"[FAIL] GameRecord with TableId='{tableId}' disappeared from database " +
                      $"while processing in GameRecord Monitor agent. " +
                      $"Abandoning finding game with new matching rules..."
            };
        }

        // Find game with new matching rules
        var procs = ProcGatherer.GetListOfAvailableProcesses(context.CancellationToken);
        int? targetGamePidValue = null;
        foreach (var p in procs)
        {
            if (!RuleMatcher.IsMatch(p, targetGame)) continue;
            targetGamePidValue = p.Pid;
            break;
        }

        if (targetGamePidValue is null)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Refreshed matching rules for auto game with TableId={id} Name='{name}' (Detected as inactive).", tableId.V, targetGame.Name);

            return OkStatusAndMessageResponse;
        }

        var gamePid = new ProcPid(targetGamePidValue.Value);

        // Create new tracking session
        var newSession = new TrackingSessions.Auto
        {
            TableId = targetGame.TableId,
            GameName = targetGame.Name,
            Pid = gamePid,
            LastTimeFlushedPlayTime = DateTime.UtcNow
        };

        state.ActiveAutoGames.TryAdd(tableId, newSession);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refreshed matching rules for auto game with TableId='{id}' Name='{name}' (Detected as active).", tableId.V, targetGame.Name);

        return OkStatusAndMessageResponse;
    }

    public override async Task<StatusResponse> EvictOldInstance(EmptyRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Eviction requested via gRPC. Triggering graceful shutdown...");

        try
        {
            await Task.Delay(200, lifetime.ApplicationStopping);
            lifetime.StopApplication();
        }
        catch (OperationCanceledException)
        {
            // Application is already shutting down
        }

        return OkStatusResponse;
    }
}