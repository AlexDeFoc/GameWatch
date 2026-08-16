using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;
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
            logger.LogInformation("[INFO] Agent started. Loading important stuff...");

        state.LoadedAutoGames = [.. GameLibrary.Instance.GetAutoGames()];

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[OK]️ Loaded auto games from db. Found={count} games", state.LoadedAutoGames.Count);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var secondsElapsed = 0;

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                // Check if refresh auto games signal was triggered
                if (state.ConsumeRefreshRequest())
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("[INFO] Reloading auto games from db...");

                    // Any ongoing foreach loops keep looking at the old snapshot until their next execution
                    state.LoadedAutoGames = [.. GameLibrary.Instance.GetAutoGames()];

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("[OK] Finished reloading auto games.");
                }

                secondsElapsed++;
                if (secondsElapsed >= 5)
                {
                    scanner.Scan();
                    secondsElapsed = 0;
                }

                heartbeatProcessor.FlushHeartbeats(forceFlushAll: false);
            }
        }
        finally
        {
            // Graceful Shutdown: force flush remaining seconds for ALL active sessions IF NEEDED
            if (!state.ActiveAutoGames.IsEmpty || !state.ActiveManualGames.IsEmpty)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[INFO] Agent shutdown requested. Performing final partial flush...");

                heartbeatProcessor.FlushHeartbeats(forceFlushAll: true);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[OK] All games saved.");
            }
        }
    }
}