using System.Collections.Concurrent;
using System.Threading;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    public ConcurrentDictionary<TableId, Pid> ActiveAutoGamesPids { get; } = [];

    public ConcurrentList<AutoGameRecord> LoadedAutoGames { get; } = [];

    public ConcurrentDictionary<Pid, TrackingSessions.Auto> ActiveAutoGames { get; } = [];

    public ConcurrentDictionary<TableId, TrackingSessions.Manual> ActiveManualGames { get; } = [];

    private CancellationTokenSource _gameListRefreshCts = new();
    public CancellationToken GameListRefreshToken => _gameListRefreshCts.Token;

    public void RequestGameListRefresh() => _gameListRefreshCts.Cancel();

    public void ResetGameListRefresh()
    {
        _gameListRefreshCts.Dispose();
        _gameListRefreshCts = new CancellationTokenSource();
    }
}