using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameWatch.Agent.GameMonitor;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Step 1: Evict old instance using a dedicated, short-lived token
        using (var evictionCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500)))
        {
            var evictResult = await Core.GameMonitorAgentIpcServer.RequestOldInstanceEvictionAsync(evictionCts.Token);

            if (!evictResult.Ok)
            {
                Console.WriteLine(evictResult.FailureReason);
            }
            else if (evictResult.InstanceWasPresent)
            {
                Console.WriteLine("[INFO] Previous agent evicted. Waiting for pipe release...");
                // Option to add a tiny Task.Delay if needed for the OS pipe teardown
                await Task.Delay(150, evictionCts.Token);
            }
            else
            {
                Console.WriteLine("[INFO] No existing agent found running. Starting fresh...");
            }
        }

        // Step 2: Initialize DB
        GameLibrary.Init("../../UserData");
        GamePresets.Init("../../AppData");
        Settings.Init("../../AppData");

        var builder = Host.CreateApplicationBuilder(args);

        // Registered state & services
        builder.Services.AddSingleton<AgentState>();
        builder.Services.AddSingleton<IpcProcessorImpl>();
        builder.Services.AddSingleton<ProcessScanner>();
        builder.Services.AddSingleton<HeartbeatProcessor>();

        builder.Services.AddHostedService<IpcSignalCollector>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        // Step 3: Run Host using its default lifecycle (Listens for Ctrl+C, StopApplication, etc.)
        await host.RunAsync();
    }
}