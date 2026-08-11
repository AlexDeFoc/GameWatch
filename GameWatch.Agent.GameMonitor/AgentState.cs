using System.Collections.Concurrent;
using System.Threading;
using GameWatch.Core;
using GameWatch.Core.GameRecords;
using GameWatch.Core.Wrappers;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    public ConcurrentDictionary<GameId, Pid> ActiveAutoGamesPids { get; } = [];

    public ConcurrentList<AutoGame> LoadedAutoGames { get; } = [];

    public ConcurrentDictionary<Pid, TrackingSessions.Auto> ActiveAutoGames { get; } = [];

    public ConcurrentDictionary<GameId, TrackingSessions.Manual> ActiveManualGames { get; } = [];

    private CancellationTokenSource _gameListRefreshCts = new();
    public CancellationToken GameListRefreshToken => _gameListRefreshCts.Token;

    public void RequestGameListRefresh() => _gameListRefreshCts.Cancel();

    public void ResetGameListRefresh()
    {
        _gameListRefreshCts.Dispose();
        _gameListRefreshCts = new CancellationTokenSource();
    }
}