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
    public override Task<IpcResponse> RemoveAutoGame(GameIdRequest request, ServerCallContext context)
    {
        var id = new GameId(request.GameId);

        if (state.ActiveAutoGames.TryRemove(state.ActiveAutoGamesPids[id], out var session))
        {
            state.ActiveAutoGamesPids.TryRemove(id, out _);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed auto game with Id={id} Name={name}", id, session.Game.Name);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Auto Game with Id={id} wasn't being tracked. Ignoring signal to remove auto Game...", id);
        }

        state.RequestGameListRefresh();

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> RemoveManualGame(GameIdRequest request, ServerCallContext context)
    {
        var id = new GameId(request.GameId);

        if (state.ActiveManualGames.TryRemove(id, out _))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Removed manual Game with Id={id}", id);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Manual Game with Id={id} wasn't being tracked. Ignoring signal to remove manual Game...", id);
        }

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> RefreshAutoGamesList(EmptyRequest request, ServerCallContext context)
    {
        state.RequestGameListRefresh();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] Refresh auto games signal received");

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> ToggleManualGame(GameIdRequest request, ServerCallContext context)
    {
        var id = new GameId(request.GameId);

        if (state.ActiveManualGames.TryRemove(id, out var session))
        {
            var elapsed = (long)(DateTime.UtcNow - session.LastTimeFlushedPlayTime).TotalSeconds;
            if (elapsed > 0)
            {
                GameLibrary.Instance.IncrementPlayTime(GameMode.Manual, id, elapsed);
            }

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Stopped manual Game Id={id}, Elapsed={elapsed}s", id, elapsed);
        }
        else
        {
            var newSession = new TrackingSessions.Manual { Id = id };
            state.ActiveManualGames.TryAdd(id, newSession);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Started manual Game Id={id}", id);
        }

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> ResetActiveManualGame(GameIdRequest request, ServerCallContext context)
    {
        var id = new GameId(request.GameId);

        if (state.ActiveManualGames.TryGetValue(id, out var session))
        {
            session.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Reset manual Game playtime with Id={id}", id);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Manual Game with Id={id} wasn't being tracked. Ignoring signal to reset manual Game playtime...", id);
        }

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> ResetActiveAutoGame(GameIdRequest request, ServerCallContext context)
    {
        var id = new GameId(request.GameId);

        if (state.ActiveAutoGames.TryGetValue(state.ActiveAutoGamesPids[id], out var session))
        {
            session.LastTimeFlushedPlayTime = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[OK] Reset auto game playtime with Id={id} Name={name}", id, session.Game.Name);
        }
        else if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("[WARN] Auto Game with Id={id} wasn't being tracked. Ignoring signal to reset auto Game playtime...", id);
        }

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> EditAutoGame(GameIdxRequest request, ServerCallContext context)
    {
        var idx = new GameIdx(request.GameIdx);
        var idOpt = GameLibrary.Instance.GetGameIdByIdx(GameMode.Auto, idx);

        if (idOpt == null)
        {
            return Task.FromResult(new IpcResponse { Success = false, Message = "Game not found in database." });
        }

        var id = idOpt.Value;

        if (state.ActiveAutoGamesPids.TryGetValue(id, out var activePid))
        {
            // TODO: check if we can merge try get value and try remove
            if (state.ActiveAutoGames.TryGetValue(activePid, out var prevSession))
            {
                var elapsed = (long)(DateTime.UtcNow - prevSession.LastTimeFlushedPlayTime).TotalSeconds;
                if (elapsed > 0)
                    GameLibrary.Instance.IncrementPlayTime(GameMode.Auto, id, elapsed);
            }

            state.ActiveAutoGames.TryRemove(activePid, out _);
        }

        // Refresh auto games list
        state.LoadedAutoGames.ReplaceAll(GameLibrary.Instance.GetAutoGames());

        if (idx.V - 1 < 0 || idx.V - 1 >= state.LoadedAutoGames.Count)
        {
            // TODO: WHAT!? it should be false, no...?
            return Task.FromResult(new IpcResponse { Success = true });
        }

        var targetGame = state.LoadedAutoGames.Get(idx);

        // Re-evaluate running processes against updated rules
        var procs = ProcGatherer.GetListOfAvailableProcesses();
        var gamePidRaw = procs.FirstOrDefault(p => RuleMatcher.IsMatch(p, targetGame))?.Pid;

        if (gamePidRaw == null)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] Refreshed edited auto game Id={id} Name='{name}' (Currently inactive).", id, targetGame.Name);

            return Task.FromResult(new IpcResponse { Success = true });
        }

        // Register new/updated active tracking session
        var gamePid = new Pid(gamePidRaw.Value);

        var newSession = new TrackingSessions.Auto
                         {
                             Game = targetGame,
                             LastTimeFlushedPlayTime = DateTime.UtcNow
                         };

        state.ActiveAutoGames.TryAdd(gamePid, newSession);
        state.ActiveAutoGamesPids.TryAdd(id, gamePid);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[OK] Refreshed active edited game with Id={id} Name='{name}'", id, targetGame.Name);

        return Task.FromResult(new IpcResponse { Success = true });
    }

    public override Task<IpcResponse> EvictAgent(EvictRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("👋 Eviction requested via gRPC. Triggering graceful shutdown...");

        _ = Task.Run(async () =>
        {
            await Task.Delay(200); // Give gRPC a moment to flush the response back to client
            lifetime.StopApplication();
        });

        return Task.FromResult(new IpcResponse { Success = true, Message = "Shutdown initiated." });
    }
}