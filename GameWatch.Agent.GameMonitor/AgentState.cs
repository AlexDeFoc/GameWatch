using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using GameWatch.Core.Types;

namespace GameWatch.Agent.GameMonitor;

public sealed class AgentState
{
    private ImmutableArray<TrackingSessions.Auto> _activeAutoGames = [];
    private ImmutableArray<TrackingSessions.Manual> _activeManualGames = [];
    private ImmutableArray<AutoGameRecord> _loadedAutoGames = [];

// Lock-free, zero-allocation array reads for heartbeat ticks
    public ImmutableArray<TrackingSessions.Auto> ActiveAutoGames => _activeAutoGames;
    public ImmutableArray<TrackingSessions.Manual> ActiveManualGames => _activeManualGames;

    // --- Active Auto Session Helpers ---
    public bool TryGetActiveAutoGame(TableId tableId, out TrackingSessions.Auto? session)
    {
        var current = _activeAutoGames;
        foreach (var s in current)
        {
            if (s.TableId != tableId) continue;
            session = s;
            return true;
        }

        session = null;
        return false;
    }

    public bool AddActiveAutoGame(TrackingSessions.Auto session)
    {
        return ImmutableInterlocked.Update(ref _activeAutoGames, static (list, item) =>
        {
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i].TableId == item.TableId) return list; // Prevent duplicates
            }

            return list.Add(item);
        }, session);
    }

    public bool RemoveActiveAutoGame(TableId tableId, [NotNullWhen(true)] out TrackingSessions.Auto? removedSession)
    {
        // Step 1: Find target session from lock-free array snapshot
        var current = _activeAutoGames;
        TrackingSessions.Auto? target = null;

        for (var i = 0; i < current.Length; i++)
        {
            if (current[i].TableId != tableId) continue;
            target = current[i];
            break;
        }

        if (target is null)
        {
            removedSession = null;
            return false;
        }

        // Step 2: Atomic update pass using matching overload Update<T, TArg>(ref array, func, arg)
        var updated = ImmutableInterlocked.Update(
            ref _activeAutoGames,
            static (list, id) =>
            {
                for (var i = 0; i < list.Length; i++)
                {
                    if (list[i].TableId == id) return list.RemoveAt(i);
                }
                return list;
            },
            tableId);

        removedSession = target;
        return updated;
    }

    public void RemoveAllActiveAutoGames() => ImmutableInterlocked.Update(ref _activeAutoGames, static _ => []);

    // --- Active Manual Session Helpers ---

    public bool TryGetActiveManualGame(TableId id, out TrackingSessions.Manual? session)
    {
        var list = _activeManualGames;
        foreach (var item in list)
        {
            if (item.TableId != id) continue;
            session = item;
            return true;
        }

        session = null;
        return false;
    }

    public bool AddActiveManualGame(TrackingSessions.Manual session)
    {
        return ImmutableInterlocked.Update(ref _activeManualGames, static (list, item) =>
        {
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i].TableId == item.TableId) return list; // Prevent duplicates
            }

            return list.Add(item);
        }, session);
    }

    public bool RemoveActiveManualGame(TableId tableId, [NotNullWhen(true)] out TrackingSessions.Manual? removedSession)
    {
        var current = _activeManualGames;
        TrackingSessions.Manual? target = null;

        for (var i = 0; i < current.Length; i++)
        {
            if (current[i].TableId != tableId) continue;
            target = current[i];
            break;
        }

        if (target is null)
        {
            removedSession = null;
            return false;
        }

        var updated = ImmutableInterlocked.Update(
            ref _activeManualGames,
            static (list, id) =>
            {
                for (var i = 0; i < list.Length; i++)
                {
                    if (list[i].TableId == id) return list.RemoveAt(i);
                }
                return list;
            },
            tableId);

        removedSession = target;
        return updated;
    }

    public void RemoveAllActiveManualGames() => ImmutableInterlocked.Update(ref _activeManualGames, static _ => []);

    // --- Loaded Auto Games Snapshot Helpers ---

    public ImmutableArray<AutoGameRecord> GetLoadedAutoGames() => _loadedAutoGames;

    public int LoadedAutoGamesCount() => _loadedAutoGames.Length;

    public void ReplaceAllAutoGames(AutoGameRecord[] newRecords)
    {
        var next = ImmutableArray.Create(newRecords);
        ImmutableInterlocked.Update(ref _loadedAutoGames, static (_, fresh) => fresh, next);
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

    public void RemoveAllAutoGames() => ImmutableInterlocked.Update(ref _loadedAutoGames, static _ => []);
}