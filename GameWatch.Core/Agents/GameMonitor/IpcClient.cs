using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;
using GrpcDotNetNamedPipes;

namespace GameWatch.Core.Agents.GameMonitor;

public static class IpcClient
{
    private const int ConnectTimeoutMs = 500;

    private static IpcProcessor.IpcProcessorClient CreateClient(IpcTarget target)
    {
        var pipeName = target switch
        {
            IpcTarget.GameWatchGameMonitorAgent => IpcConstants.GameMonitorAgentPipeName,
            _ => throw new NotImplementedException()
        };

        var channel = new NamedPipeChannel(".", pipeName, new NamedPipeChannelOptions
                                                          {
                                                              ConnectionTimeout = ConnectTimeoutMs
                                                          });

        return new IpcProcessor.IpcProcessorClient(channel);
    }

    public static async Task<bool> SendResetActiveManualGameSignalAsync(IpcTarget target, GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ResetActiveManualGameAsync(new GameIdRequest {GameId = gameId.V}, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendResetActiveAutoGameSignalAsync(IpcTarget target, GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ResetActiveAutoGameAsync(new GameIdRequest {GameId = gameId.V}, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendRefreshSignalForAutoGamesListAsync(IpcTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.RefreshAutoGamesListAsync(new EmptyRequest(), cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendToggleManualGameSignalAsync(IpcTarget target, GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ToggleManualGameAsync(new GameIdRequest { GameId = gameId.V }, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false; // Agent offline or timed out
        }
    }

    public static async Task<bool> SendRemoveManualGameSignalAsync(IpcTarget target, GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.RemoveManualGameAsync(new GameIdRequest {GameId = gameId.V}, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendRemoveAutoGameSignalAsync(IpcTarget target, GameId gameId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.RemoveAutoGameAsync(new GameIdRequest {GameId = gameId.V}, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendEditAutoGameSignalAsync(IpcTarget target, GameIdx gameIdx, bool wasGameActive, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.EditAutoGameAsync(new EditAutoGameRequest { GameIdx = gameIdx.V, WasGameActive = wasGameActive }, cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> SendEvictAgentSignalAsync(IpcTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.EvictAgentAsync(new EvictRequest(), cancellationToken: cancellationToken);
            return response.Success;
        }
        catch
        {
            // Throws if channel connection fails or times out (meaning no active agent was listening)
            return false;
        }
    }
}