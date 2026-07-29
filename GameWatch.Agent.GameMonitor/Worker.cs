using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class Worker(
    AgentState state,
    ProcessScanner scanner,
    HeartbeatProcessor heartbeatProcessor,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        state.LoadedAutoGames = DbFactory.GameLibrary.GetAutoGamesWithDetailsWithIdInsteadOfPosIdx();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[Agent] Initialized with {Count} auto-game rule(s) from SQLite.", state.LoadedAutoGames.Count);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                // 1. IPC Refresh check
                if (state.GameListRefreshToken.IsCancellationRequested)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("[Agent] Refresh requested via IPC. Reloading auto-game rules...");
                    state.LoadedAutoGames = DbFactory.GameLibrary.GetAutoGamesWithDetailsWithIdInsteadOfPosIdx();
                    state.ResetGameListRefresh();
                }

                // 2. Scan active process starts/stops (handles immediate stop flushes)
                scanner.Scan();

                // 3. Flush 60s increments for sessions that reached the threshold
                heartbeatProcessor.FlushHeartbeats(forceFlushAll: false);
            }
        }
        finally
        {
            // 4. Graceful Shutdown: force flush remaining seconds for ALL active sessions
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[Agent] Shutdown requested. Performing final partial flush...");

            heartbeatProcessor.FlushHeartbeats(forceFlushAll: true);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[Agent] Playtime saved successfully. Host shutting down.");
        }
    }
}