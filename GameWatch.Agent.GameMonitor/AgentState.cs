using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GameWatch.Core.Dto.GameRecords;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    public List<AutoGame> LoadedAutoGames { get; set; } = [];

    // Key = Process PID, Value = Active Tracked Session
    public ConcurrentDictionary<int, TrackedSession> ActiveAutoGames { get; } = new();

    // Key = Game ID, Value = Active Tracked Session
    public ConcurrentDictionary<int, TrackedSession> ActiveManualGames { get; } = new();

    private CancellationTokenSource _gameListRefreshCts = new();
    public CancellationToken GameListRefreshToken => _gameListRefreshCts.Token;

    public void RequestGameListRefresh() => _gameListRefreshCts.Cancel();

    public void ResetGameListRefresh()
    {
        _gameListRefreshCts.Dispose();
        _gameListRefreshCts = new CancellationTokenSource();
    }
}