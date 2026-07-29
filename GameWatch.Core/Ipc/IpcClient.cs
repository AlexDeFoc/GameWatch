using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameWatch.Core.Ipc;

public static class IpcClient
{
    private const int ConnectTimeoutMs = 500; // 500ms timeout keeps Client snappy

    /// <summary>
    /// Sends reset for active manual game signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendResetActiveManualGameSignalAsync(IpcTarget target, int gameId, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, $"{IpcConstants.CommandResetActiveManualGame} {gameId}", cancellationToken);
    }

    /// <summary>
    /// Sends reset for active auto game signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendResetActiveAutoGameSignalAsync(IpcTarget target, int gameId, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, $"{IpcConstants.CommandResetActiveAutoGame} {gameId}", cancellationToken);
    }

    /// <summary>
    /// Sends a refresh signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendRefreshSignalAsync(IpcTarget target, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, IpcConstants.CommandRefreshAutoGamesList, cancellationToken);
    }

    /// <summary>
    /// Sends toggle manual game signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendToggleManualGameSignalAsync(IpcTarget target, int gameId, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, $"{IpcConstants.CommandToggleManualGame} {gameId}", cancellationToken);
    }

    /// <summary>
    /// Sends remove manual game signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendRemoveManualGameSignalAsync(IpcTarget target, int gameId, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, $"{IpcConstants.CommandRemoveManualGame} {gameId}", cancellationToken);
    }

    /// <summary>
    /// Sends remove auto game signal to the running background Agent.
    /// Returns true if delivered successfully; false if the Agent host is offline or unreachable.
    /// </summary>
    public static async Task<bool> SendRemoveAutoManualGameSignalAsync(IpcTarget target, int gameId, CancellationToken cancellationToken)
    {
        return await SendCommandAsync(target, $"{IpcConstants.CommandRemoveAutoGame} {gameId}", cancellationToken);
    }

    private static string GetIpcTargetPipeName(IpcTarget target)
    {
        return target switch
        {
            IpcTarget.GameWatchGameMonitorAgent => IpcConstants.GameMonitorAgentPipeName,
            _ => throw new NotImplementedException()
        };
    }

    private static async Task<bool> SendCommandAsync(IpcTarget target, string command, CancellationToken cancellationToken)
    {
        try
        {
            await using var client = new NamedPipeClientStream(serverName: ".",
                                                               pipeName: GetIpcTargetPipeName(target),
                                                               direction: PipeDirection.Out,
                                                               options: PipeOptions.Asynchronous);

            // Connect with a strict timeout so the Client doesn't hang if agent host is stopped
            await client.ConnectAsync(ConnectTimeoutMs, cancellationToken);

            await using var writer = new StreamWriter(client, Encoding.UTF8, leaveOpen: true);
            await writer.WriteLineAsync(command.AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);

            return true;
        }
        catch (Exception ex) when (ex is TimeoutException or IOException or OperationCanceledException)
        {
            // Agent is either not running, busy, or shut down
            return false;
        }
    }
}