using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core;
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
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("ℹ️ Agent started. Loading important stuff...");

        state.LoadedAutoGames.ReplaceAll(DbMng.GameLibrary.GetAutoGames());

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("✅️ Loaded auto games from db. Found={count} games", state.LoadedAutoGames.Count);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                // Check if refresh auto games signal was triggered
                if (state.GameListRefreshToken.IsCancellationRequested)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("ℹ️ Reloading auto games from db...");
                    state.LoadedAutoGames.ReplaceAll(DbMng.GameLibrary.GetAutoGames());
                    logger.LogInformation("✅ Finished reloading auto games from db...");
                    state.ResetGameListRefresh();
                }

                // Perform tasks upon tick finish (5 seconds)
                scanner.Scan();

                // Flush 60s increments for sessions that reached the threshold
                heartbeatProcessor.FlushHeartbeats(forceFlushAll: false);
            }
        }
        finally
        {
            // Graceful Shutdown: force flush remaining seconds for ALL active sessions IF NEEDED
            if (!state.ActiveAutoGames.IsEmpty || !state.ActiveManualGames.IsEmpty)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("ℹ️ Agent shutdown requested. Performing final partial flush...");

                heartbeatProcessor.FlushHeartbeats(forceFlushAll: true);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("✅ All games saved.");
            }
        }
    }
}