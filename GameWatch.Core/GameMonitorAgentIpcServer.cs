using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core.Types;
using GrpcDotNetNamedPipes;

namespace GameWatch.Core;

public static class GameMonitorAgentIpcServer
{
    private const int ConnectTimeoutMs = 500;

    public static async Task<StatusAndFailureMsgResult> NotifyThatManualGameGotResetAsync(TableId tableId, string gameName, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.ResetActiveManualGameAsync(new GameIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken);
            return !response.Ok
                ? new StatusAndFailureMsgResult(FailureReason: response.Msg)
                : new StatusAndFailureMsgResult(Ok: response.Ok);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotResetAsync(TableId tableId, string gameName, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.ResetActiveAutoGameAsync(new GameIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken);
            return !response.Ok
                ? new StatusAndFailureMsgResult(FailureReason: response.Msg)
                : new StatusAndFailureMsgResult(Ok: response.Ok);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<StatusAndFailureMsgResult> RequestToRefreshAutoGamesCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            await client.RefreshAutoGamesListAsync(new EmptyRequest(), cancellationToken: cancellationToken);
            return new StatusAndFailureMsgResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<ToggleManualGameResult> RequestToToggleManualGameAsync(TableId tableId, string gameName, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.ToggleManualGameAsync(new GameIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken);
            return !response.Ok
                ? new ToggleManualGameResult(FailureReason: response.FailureReason)
                : new ToggleManualGameResult(Ok: response.Ok, StartedGame: response.StartedGame);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ToggleManualGameResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new ToggleManualGameResult(FailureReason: $"""
                                                              [FAIL] Unhandled exception occured
                                                              [Exception msg] {ex.Message}
                                                              [Stack trace - START]
                                                              {ex.StackTrace}
                                                              [Stack trace - END]
                                                              """);
        }
    }

    public static async Task<StatusAndFailureMsgResult> NotifyThatManualGameGotRemovedAsync(TableId tableId, string gameName, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            await client.RemoveManualGameAsync(new GameIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken);
            return new StatusAndFailureMsgResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotRemovedAsync(TableId tableId, string gameName, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            await client.RemoveAutoGameAsync(new GameIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken);
            return new StatusAndFailureMsgResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotEditedAsync(TableId tableId, string gameName, bool matchingRulesChanged, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            var response = await client.EditAutoGameAsync(new EditGameRequest { TableId = tableId.V, GameName = gameName, MatchingRulesChanged = matchingRulesChanged }, cancellationToken: cancellationToken);

            return !response.Ok
                ? new StatusAndFailureMsgResult(FailureReason: response.Msg)
                : new StatusAndFailureMsgResult(Ok: response.Ok);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new StatusAndFailureMsgResult(FailureReason: $"""
                                                                 [FAIL] Unhandled exception occured
                                                                 [Exception msg] {ex.Message}
                                                                 [Stack trace - START]
                                                                 {ex.StackTrace}
                                                                 [Stack trace - END]
                                                                 """);
        }
    }

    public static async Task<EvictOldInstanceResult> RequestOldInstanceEvictionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient();
            await client.EvictOldInstanceAsync(new EmptyRequest(), cancellationToken: cancellationToken);
            return new EvictOldInstanceResult(Ok: true);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.Cancelled or Grpc.Core.StatusCode.Unavailable)
        {
            // No server was actively listening on the pipe
            return new EvictOldInstanceResult(Ok: true, InstanceWasPresent: false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new EvictOldInstanceResult(FailureReason: $"[WARN] Operation cancelled. Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new EvictOldInstanceResult(FailureReason: $"""
                                                              [FAIL] Unhandled exception occured
                                                              [Exception msg] {ex.Message}
                                                              [Stack trace - START]
                                                              {ex.StackTrace}
                                                              [Stack trace - END]
                                                              """);
        }
    }

    private static IpcProcessor.IpcProcessorClient CreateClient()
    {
        const string pipeName = IpcConstants.GameMonitorAgentPipeName;

        var channel = new NamedPipeChannel(".", pipeName, new NamedPipeChannelOptions
        {
            ConnectionTimeout = ConnectTimeoutMs
        });

        return new IpcProcessor.IpcProcessorClient(channel);
    }

    // Results
    public record struct ToggleManualGameResult(bool Ok = false, bool StartedGame = false, string? FailureReason = null);

    public record struct StatusAndFailureMsgResult(bool Ok = false, string? FailureReason = null);

    public record struct EvictOldInstanceResult(bool Ok = false, bool InstanceWasPresent = false, string? FailureReason = null);
}