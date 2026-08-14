using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;
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

    public static async Task<StatusAndFailureMsgResult> NotifyAboutManualGameResetAsync(IpcTarget target, TableId tableId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ResetActiveManualGameAsync(new TableIdRequest { TableId = tableId.V }, cancellationToken: cancellationToken);
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

    public static async Task<StatusAndFailureMsgResult> NotifyAboutAutoGameResetAsync(IpcTarget target, TableId tableId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ResetActiveAutoGameAsync(new TableIdRequest { TableId = tableId.V }, cancellationToken: cancellationToken);
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

    public static async Task<StatusAndFailureMsgResult> SendRefreshSignalForAutoGamesListAsync(IpcTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
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

    public static async Task<ToggleManualGameResult> SendToggleManualGameSignalAsync(IpcTarget target, TableId tableId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.ToggleManualGameAsync(new TableIdRequest { TableId = tableId.V }, cancellationToken: cancellationToken);
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

    public static async Task<StatusAndFailureMsgResult> NotifyAboutManualGameRemovalAsync(IpcTarget target, TableId tableId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            await client.RemoveManualGameAsync(new TableIdRequest { TableId = tableId.V }, cancellationToken: cancellationToken);
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

    public static async Task<StatusAndFailureMsgResult> NotifyAboutAutoGameRemovalAsync(IpcTarget target, TableId tableId, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            await client.RemoveAutoGameAsync(new TableIdRequest { TableId = tableId.V }, cancellationToken: cancellationToken);
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

    public static async Task<StatusAndFailureMsgResult> NotifyAboutEditAutoGameAsync(IpcTarget target, TableId tableId, bool matchingRulesChanged, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.EditAutoGameAsync(new EditGameRequest { TableId = tableId.V, MatchingRulesChanged = matchingRulesChanged }, cancellationToken: cancellationToken);

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

    public static async Task<StatusAndFailureMsgResult> SendEvictAgentSignalAsync(IpcTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var client = CreateClient(target);
            var response = await client.EvictAgentAsync(new EvictRequest(), cancellationToken: cancellationToken);
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

    public record struct ToggleManualGameResult(bool Ok = false, bool StartedGame = false, string? FailureReason = null);

    public record struct StatusAndFailureMsgResult(bool Ok = false, string? FailureReason = null);
}