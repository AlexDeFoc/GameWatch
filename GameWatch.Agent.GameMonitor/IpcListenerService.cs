using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class IpcListenerService(AgentState state, ILogger<IpcListenerService> logger) : BackgroundService
{
    private const string PipeName = Core.Ipc.IpcConstants.GameMonitorAgentPipeName;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[IPC Service] Starting Named Pipe listener on '{PipeName}'...", PipeName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a pipe instance for each incoming connection
                await using var pipeServer = new NamedPipeServerStream(PipeName,
                                                                       PipeDirection.In,
                                                                       NamedPipeServerStream.MaxAllowedServerInstances,
                                                                       PipeTransmissionMode.Byte,
                                                                       PipeOptions.Asynchronous);

                // Wait for incoming Client signal
                await pipeServer.WaitForConnectionAsync(stoppingToken);

                using var reader = new StreamReader(pipeServer, Encoding.UTF8);
                var requestCommand = await reader.ReadLineAsync(stoppingToken);

                if (!string.IsNullOrWhiteSpace(requestCommand))
                {
                    ProcessCommand(requestCommand.Trim());
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
                // Graceful host shutdown
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "[IPC ERROR] Exception while reading client signal.");
            }
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[IPC] Named Pipe Listener stopped.");
    }

    private void ProcessCommand(string command)
    {
        var parts = command.Split(' ', 2);
        var action = parts[0].ToUpperInvariant();
        var payload = parts.Length > 1 ? parts[1] : string.Empty;

        switch (action)
        {
            case Core.Ipc.IpcConstants.CommandRemoveAutoGame:
            {
                if (int.TryParse(payload, out var id))
                {
                    var gamePid = state.ActiveAutoGames.FirstOrDefault(kvp => kvp.Value.GameId == id).Key;

                    if (gamePid != 0 && state.ActiveAutoGames.TryRemove(gamePid, out _))
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Removed auto tracking for game (Pid={GamePid} Id={Id}) (No playtime save executed)", gamePid, id);
                    }
                    else if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError("[IPC Error] Provided auto game Id={Id} wasn't actively being tracked", id);
                    }

                    state.RequestGameListRefresh();
                }

                break;
            }

            case Core.Ipc.IpcConstants.CommandRemoveManualGame:
            {
                if (int.TryParse(payload, out var id))
                {
                    if (state.ActiveManualGames.TryRemove(id, out _))
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Stopped manual tracking for game Id={Id} (No playtime save executed)", id);
                    }
                    else if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError("[IPC Error] Provided manual game Id={Id} wasn't actively being tracked", id);
                    }
                }

                break;
            }

            case Core.Ipc.IpcConstants.CommandRefreshAutoGamesList:
            {
                state.RequestGameListRefresh();
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[IPC] Refresh signal received. Notifying Worker...");
                break;
            }

            case Core.Ipc.IpcConstants.CommandToggleManualGame:
            {
                if (int.TryParse(payload, out var id))
                {
                    if (state.ActiveManualGames.TryRemove(id, out var session))
                    {
                        // Stop tracking: flush remaining delta time
                        var elapsed = (int)(DateTime.UtcNow - session.LastFlushedUtc).TotalSeconds;
                        if (elapsed > 0)
                        {
                            DbFactory.GameLibrary.IncrementPlayTime(GameMode.Manual, id, elapsed);
                        }

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Stopped manual tracking for game Id={Id} (+{Seconds}s flushed)", id, elapsed);
                    }
                    else
                    {
                        // Start tracking
                        var newSession = new TrackedSession
                                         {
                                             GameId = id,
                                             Mode = GameMode.Manual,
                                             LastFlushedUtc = DateTime.UtcNow
                                         };
                        state.ActiveManualGames.TryAdd(id, newSession);

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Started manual tracking for game Id={Id}", id);
                    }
                }

                break;
            }

            case Core.Ipc.IpcConstants.CommandResetActiveManualGame:
            {
                if (int.TryParse(payload, out var id))
                {
                    if (state.ActiveManualGames.TryGetValue(id, out var session))
                    {
                        session.LastFlushedUtc = DateTime.UtcNow;

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Reset session timer clock for active Manual Game Id={Id}", id);
                    }
                    else
                    {
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.LogError("[IPC Error] Provided manual game Id={Id} wasn't actively being tracked", id);
                    }
                }

                break;
            }

            case Core.Ipc.IpcConstants.CommandResetActiveAutoGame:
            {
                if (int.TryParse(payload, out var id))
                {
                    var gamePid = state.ActiveAutoGames.FirstOrDefault(kvp => kvp.Value.GameId == id).Key;

                    if (gamePid != 0 && state.ActiveAutoGames.TryGetValue(gamePid, out var session))
                    {
                        session.LastFlushedUtc = DateTime.UtcNow;

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("[IPC] Reset session timer clock for active Auto Game Id={Id}", id);
                    }
                    else
                    {
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.LogError("[IPC Error] Provided auto game Id={Id} wasn't actively being tracked", id);
                    }
                }

                break;
            }

            default:
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning("[IPC] Received unrecognized signal: {Command}", command);
                break;
            }
        }
    }
}