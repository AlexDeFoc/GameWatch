using System;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor.Ipc.Grpc;
using GameWatch.Core.Types;
using Grpc.Core;
using GrpcDotNetNamedPipes;

namespace GameWatch.Core;

public static class GameMonitorAgentIpcServer
{
    public static Task<StatusAndFailureMsgResult> NotifyThatManualGameGotResetAsync(TableId tableId, string gameName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.ResetActiveManualGameAsync(new TableIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken),
            res => !res.Ok ? new StatusAndFailureMsgResult(FailureReason: res.Msg) : new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotResetAsync(TableId tableId, string gameName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.ResetActiveAutoGameAsync(new TableIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken),
            res => !res.Ok ? new StatusAndFailureMsgResult(FailureReason: res.Msg) : new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<StatusAndFailureMsgResult> RequestToTrackNewlyAddedAutoGameAsync(AutoGameDto game, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.TrackNewlyAddedAutoGameAsync(new AutoGameDtoRequest
            {
                TableId = game.TableId,
                GameName = game.Name,
                PlayTimeSec = game.PlayTimeSec,
                WindowTitle = game.WindowTitle ?? string.Empty,
                WindowRule = game.WindowRule ?? string.Empty,
                FilePath = game.FilePath ?? string.Empty,
                PathRule = game.PathRule ?? string.Empty
            }, cancellationToken: cancellationToken),
            res => !res.Ok ? new StatusAndFailureMsgResult(FailureReason: res.Msg) : new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<StatusAndFailureMsgResult> RequestToStopTrackingAllGamesAsync(bool stopTrackingAutoGames, bool stopTrackingManualGames, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.StopTrackingAllGamesAsync(new StopTrackingAllGamesRequest
            {
                StopTrackingAutoGames = stopTrackingAutoGames,
                StopTrackingManualGames = stopTrackingManualGames
            }, cancellationToken: cancellationToken),
            _ => new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<ToggleManualGameResult> RequestToToggleManualGameAsync(TableId tableId, string gameName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.ToggleManualGameAsync(new TableIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken),
            res => !res.Ok ? new ToggleManualGameResult(FailureReason: res.FailureReason) : new ToggleManualGameResult(Ok: true, StartedGame: res.StartedGame)
        );

    public static Task<StatusAndFailureMsgResult> NotifyThatManualGameGotRemovedAsync(TableId tableId, string gameName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.RemoveManualGameAsync(new TableIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken),
            _ => new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotRemovedAsync(TableId tableId, string gameName, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.RemoveAutoGameAsync(new TableIdAndNameRequest { TableId = tableId.V, GameName = gameName }, cancellationToken: cancellationToken),
            _ => new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<StatusAndFailureMsgResult> NotifyThatAutoGameGotEditedAsync(TableId tableId, string gameName, bool matchingRulesChanged, CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.EditAutoGameAsync(new EditGameRequest { TableId = tableId.V, GameName = gameName, MatchingRulesChanged = matchingRulesChanged }, cancellationToken: cancellationToken),
            res => !res.Ok ? new StatusAndFailureMsgResult(FailureReason: res.Msg) : new StatusAndFailureMsgResult(Ok: true)
        );

    public static Task<EvictOldInstanceResult> RequestOldInstanceEvictionAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(
            client => client.EvictOldInstanceAsync(new EmptyRequest(), cancellationToken: cancellationToken),
            _ => new EvictOldInstanceResult(Ok: true, InstanceWasPresent: true),
            // Custom failure handler specifically for eviction checks
            ex => ex.StatusCode is StatusCode.Cancelled or StatusCode.Unavailable
                ? new EvictOldInstanceResult(Ok: true, InstanceWasPresent: false)
                : new EvictOldInstanceResult(FailureReason: $"[WARN] gRPC call failed ({ex.StatusCode}): {ex.Status.Detail}")
        );

    private static readonly Lazy<IpcProcessor.IpcProcessorClient> SharedClient = new(() =>
    {
        const string pipeName = IpcConstants.GameMonitorAgentPipeName;
        const int connectTimeoutMs = 500;

        var channel = new NamedPipeChannel(".", pipeName, new NamedPipeChannelOptions { ConnectionTimeout = connectTimeoutMs });
        return new IpcProcessor.IpcProcessorClient(channel);
    });

    private static async Task<TResult> ExecuteAsync<TResponse, TResult>(
        Func<IpcProcessor.IpcProcessorClient, AsyncUnaryCall<TResponse>> call,
        Func<TResponse, TResult> mapResult,
        Func<RpcException, TResult>? customRpcFailureMapper = null)
        where TResult : struct
    {
        try
        {
            var response = await call(SharedClient.Value);
            return mapResult(response);
        }
        catch (RpcException ex)
        {
            // Allow caller to intercept specific status codes (e.g. Eviction missing server)
            if (customRpcFailureMapper is not null)
                return customRpcFailureMapper(ex);

            return ex.StatusCode switch
            {
                StatusCode.Unavailable => CreateFailure<TResult>("[WARN] Failed to notify game monitor agent. Is the agent running?"),
                StatusCode.Cancelled => CreateFailure<TResult>("[WARN] Request was cancelled by the client or server."),
                _ => CreateFailure<TResult>($"[WARN] gRPC call failed ({ex.StatusCode}): {ex.Status.Detail}")
            };
        }
        catch (Exception ex)
        {
            return CreateFailure<TResult>($"[FAIL] Unhandled exception: {ex.Message}");
        }
    }

    private static TResult CreateFailure<TResult>(string msg) where TResult : struct
    {
        if (typeof(TResult) == typeof(ToggleManualGameResult))
            return (TResult)(object)new ToggleManualGameResult(FailureReason: msg);

        if (typeof(TResult) == typeof(EvictOldInstanceResult))
            return (TResult)(object)new EvictOldInstanceResult(FailureReason: msg);

        return (TResult)(object)new StatusAndFailureMsgResult(FailureReason: msg);
    }

    // Results
    public record struct ToggleManualGameResult(bool Ok = false, bool StartedGame = false, string? FailureReason = null);

    public record struct StatusAndFailureMsgResult(bool Ok = false, string? FailureReason = null);

    public record struct EvictOldInstanceResult(bool Ok = false, bool InstanceWasPresent = false, string? FailureReason = null);
}