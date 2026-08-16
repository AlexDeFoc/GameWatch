using System;
using System.Collections;
using System.Collections.Generic;

namespace GameWatch.Core.Types;

public class TableIdMap() : IEnumerable<TableId>
{
    private readonly List<TableId> _list = [];
    private readonly Dictionary<TableId, DisplayId> _reverseLookup = new();

    // Optional constructor if you want to initialize directly from a list
    public TableIdMap(IEnumerable<TableId> tableIds) : this()
    {
        AddRange(tableIds);
    }

    /// <summary>
    /// Bulk adds a collection of TableIds, auto-assigning sequential DisplayIds.
    /// </summary>
    private void AddRange(IEnumerable<TableId> tableIds)
    {
        // Optimize underlying collections if count is known upfront
        if (tableIds is IReadOnlyCollection<TableId> collection)
        {
            _list.Capacity = Math.Max(_list.Capacity, _list.Count + collection.Count);
            _reverseLookup.EnsureCapacity(_reverseLookup.Count + collection.Count);
        }

        foreach (var tableId in tableIds)
        {
            Add(tableId);
        }
    }

    /// <summary>
    /// Removes an entry by TableId and re-indexes all subsequent DisplayIds so they stay contiguous.
    /// </summary>
    public bool Remove(TableId tableId)
    {
        if (!_reverseLookup.TryGetValue(tableId, out var displayId))
            return false;

        var indexToRemove = displayId.V - 1;

        _list.RemoveAt(indexToRemove);
        _reverseLookup.Remove(tableId);

        // Re-index remaining shifted items (DisplayId becomes i + 1)
        for (var i = indexToRemove; i < _list.Count; i++)
        {
            _reverseLookup[_list[i]] = new DisplayId(i + 1);
        }

        return true;
    }

    /// <summary>
    /// Removes an entry by its DisplayId (1-based index) and re-indexes subsequent DisplayIds.
    /// </summary>
    public bool RemoveAt(DisplayId displayId)
    {
        var indexToRemove = displayId.V - 1;

        if (indexToRemove < 0 || indexToRemove >= _list.Count)
            return false;

        var tableId = _list[indexToRemove];

        _list.RemoveAt(indexToRemove);
        _reverseLookup.Remove(tableId);

        // Re-index remaining shifted items
        for (var i = indexToRemove; i < _list.Count; i++)
        {
            _reverseLookup[_list[i]] = new DisplayId(i + 1);
        }

        return true;
    }

    public int Count => _list.Count;

    /// <summary>
    /// Adds a TableId and automatically assigns the next sequential 1-based DisplayId.
    /// </summary>
    public DisplayId Add(TableId tableId)
    {
        if (_reverseLookup.ContainsKey(tableId))
        {
            throw new ArgumentException($"TableId {tableId} is already mapped.", nameof(tableId));
        }

        // Calculate 1-based DisplayId from current length
        var displayId = new DisplayId(_list.Count + 1);

        _list.Add(tableId);
        _reverseLookup.Add(tableId, displayId);

        return displayId;
    }

    // O(1) DisplayId -> TableId (List Indexing)
    public TableId this[DisplayId displayId] => _list[displayId.V - 1];

    // O(1) TableId -> DisplayId (Dictionary Lookup)
    public DisplayId this[TableId tableId] => _reverseLookup[tableId];

    public bool TryGetDisplayId(TableId tableId, out DisplayId displayId)
        => _reverseLookup.TryGetValue(tableId, out displayId);

    public bool Contains(TableId tableId) => _reverseLookup.ContainsKey(tableId);

    public bool Contains(DisplayId displayId) => _reverseLookup.ContainsValue(displayId);

    public void Clear()
    {
        _list.Clear();
        _reverseLookup.Clear();
    }

    public IEnumerator<TableId> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}