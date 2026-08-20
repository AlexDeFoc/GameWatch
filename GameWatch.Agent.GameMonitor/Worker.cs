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

        state.ReplaceAllAutoGames(await GameLibrary.Instance.GetAutoGamesAsync(stoppingToken));

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[OK]️ Loaded auto games from db. Found={count} games", state.LoadedAutoGamesCount());

        await scanner.ScanAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var secondsElapsed = 0;

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                secondsElapsed++;
                if (secondsElapsed >= 10)
                {
                    await scanner.ScanAsync(stoppingToken);
                    secondsElapsed = 0;
                }

                await heartbeatProcessor.FlushHeartbeats(stoppingToken, forceFlushAll: false);
            }
        }
        finally
        {
            // Graceful Shutdown: force flush remaining seconds for ALL active sessions IF NEEDED
            if (!state.ActiveAutoGames.IsEmpty || !state.ActiveManualGames.IsEmpty)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[INFO] Agent shutdown requested. Performing final partial flush...");

                using var shutdownCtSrc = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                await heartbeatProcessor.FlushHeartbeats(shutdownCtSrc.Token, forceFlushAll: true);

                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("[OK] All games saved.");
            }
        }
    }
}