using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    public ConcurrentDictionary<TableId, TrackingSessions.Auto> ActiveAutoGames { get; } = [];
    public ConcurrentDictionary<TableId, TrackingSessions.Manual> ActiveManualGames { get; } = [];

    public ImmutableList<AutoGameRecord> LoadedAutoGames
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value);
    } = ImmutableList<AutoGameRecord>.Empty;

    private int _refreshRequested;

    public void RequestGameListRefresh() => Interlocked.Exchange(ref _refreshRequested, 1);

    /// <summary> Returns true if a refresh was pending, and atomically resets it to 0 </summary>
    public bool ConsumeRefreshRequest() => Interlocked.Exchange(ref _refreshRequested, 0) == 1;
}