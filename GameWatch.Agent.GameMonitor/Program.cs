using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameWatch.Agent.GameMonitor;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += CancelHandler;

        try
        {
            // Step 1: Evict old instance using a dedicated, short-lived token
            using (var evictionCts = CancellationTokenSource.CreateLinkedTokenSource(
                       cts.Token,
                       new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token))
            {
                var evictResult = await GameMonitorAgentIpcServer.RequestOldInstanceEvictionAsync(evictionCts.Token);

                if (!evictResult.Ok)
                {
                    Console.WriteLine(evictResult.FailureReason);
                }
                else if (evictResult.InstanceWasPresent)
                {
                    Console.WriteLine("[INFO] Previous agent evicted. Waiting for pipe release...");
                    await Task.Delay(150, evictionCts.Token);
                }
                else
                {
                    Console.WriteLine("[INFO] No existing agent found running. Starting fresh...");
                }
            }


            // Step 2: Initialize DB
            await GameLibrary.CreateAndInitAsync("../../UserData", cts.Token);
            await GamePresets.CreateAndInitAsync("../../AppData", cts.Token);
            await Settings.GameMonitorAgent.CreateAndInitAsync("../../AppData", cts.Token);

            var builder = Host.CreateApplicationBuilder(args);

            // Registered state & services
            builder.Services.AddSingleton<AgentState>();
            builder.Services.AddSingleton<IpcProcessorImpl>();
            builder.Services.AddSingleton<ProcessScanner>();
            builder.Services.AddSingleton<HeartbeatProcessor>();

            builder.Services.AddHostedService<IpcSignalCollector>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            // Step 3: Run Host. host.RunAsync listens to host shutdown signals
            // AND respects cts.Token if canceled early.
            await host.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[INFO] Agent startup or execution was cancelled.");
        }
        finally
        {
            Console.CancelKeyPress -= CancelHandler;
        }

        return;

        void CancelHandler(object? _, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Prevent abrupt OS exit so cleanup/finally runs
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel();
        }
    }
}