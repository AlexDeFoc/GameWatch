using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Ipc;
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
            var evicted = await IpcClient.SendEvictAgentSignalAsync(IpcTarget.GameWatchGameMonitorAgent, evictionCts.Token);
            if (evicted)
            {
                Console.WriteLine("♻️ Previous agent evicted. Waiting for pipe release...");
            }
        }

        // Step 2: Initialize DB
        DbMng.GameLibrary.InitializeDatabase("../../UserData");
        DbMng.GameLibraryPresets.InitializeDatabase("../../AppData");

        var builder = Host.CreateApplicationBuilder(args);

        // Registered state & services
        builder.Services.AddSingleton<AgentState>();
        builder.Services.AddSingleton<IpcProcessorImpl>();
        builder.Services.AddSingleton<ProcessScanner>();
        builder.Services.AddSingleton<HeartbeatProcessor>();

        builder.Services.AddHostedService<IpcServer>();
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        // Step 3: Run Host using its default lifecycle (Listens for Ctrl+C, StopApplication, etc.)
        await host.RunAsync();
    }
}