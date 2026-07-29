using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameWatch.Agent.GameMonitor;

public static class Program
{
    public static void Main(string[] args)
    {
        // Init DB paths and pragmas first
        Core.Helpers.DbFactory.GameLibrary.InitializeDatabase("../../UserData");

        var builder = Host.CreateApplicationBuilder(args);

        // Register shared state as Singleton
        builder.Services.AddSingleton<AgentState>();

        // Register domain processors as Singleton
        builder.Services.AddSingleton<ProcessScanner>();
        builder.Services.AddSingleton<HeartbeatProcessor>();

        // Register both background hosted services
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddHostedService<IpcListenerService>();

        var host = builder.Build();
        host.Run();
    }
}