using System.Collections.Concurrent;
using System.Collections.Immutable;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    public ConcurrentDictionary<TableId, TrackingSessions.Auto> ActiveAutoGames { get; } = [];
    public ConcurrentDictionary<TableId, TrackingSessions.Manual> ActiveManualGames { get; } = [];

    private ImmutableArray<AutoGameRecord> _loadedAutoGames = [];

    public ImmutableArray<AutoGameRecord> GetLoadedAutoGames() => _loadedAutoGames;

    public int LoadedAutoGamesCount() => _loadedAutoGames.Length;

    public void ReplaceAllAutoGames(AutoGameRecord[] newRecords)
    {
        _loadedAutoGames = [.. newRecords];
    }

    public void AddAutoGame(AutoGameRecord record)
    {
        ImmutableInterlocked.Update(ref _loadedAutoGames, static (list, item) => list.Add(item), record);
    }

    public void RemoveAutoGame(TableId tableId)
    {
        ImmutableInterlocked.Update(ref _loadedAutoGames, static (list, id) =>
        {
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i].TableId == id) return list.RemoveAt(i);
            }
            return list;
        }, tableId);
    }

    public void RemoveAllAutoGames()
    {
        _loadedAutoGames = [];
    }
}