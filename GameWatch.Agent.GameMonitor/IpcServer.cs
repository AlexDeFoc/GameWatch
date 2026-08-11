using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GrpcDotNetNamedPipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameWatch.Agent.GameMonitor;

public sealed class IpcServer(IpcProcessorImpl ipcProcessor, ILogger<IpcServer> logger) : BackgroundService
{
    private readonly NamedPipeServer _server = new(Core.Ipc.IpcConstants.GameMonitorAgentPipeName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IpcProcessor.BindService(_server.ServiceBinder, ipcProcessor);
        _server.Start();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[INFO] gRPC IPC Server started on pipe:'{PipeName}'", Core.Ipc.IpcConstants.GameMonitorAgentPipeName);

        try
        {
            // Keep service alive until shutdown is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful stop
        }
        finally
        {
            _server.Dispose();
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("[INFO] gRPC IPC server shut down.");
        }
    }
}