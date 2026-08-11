using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GameWatch.Core.Wrappers;

namespace GameWatch.Core;

public class ConcurrentList<T> : IEnumerable<T>
{
    private readonly List<T> _list = [];
    private readonly Lock _lock = new();

    public void RemoveAt(GameIdx idx) => RemoveAt(idx.V);

    public T Get(GameIdx idx) => Get(idx.V);

    public T this[int idx] => Get(idx);

    public T this[GameIdx idx] => Get(idx);

    public void Clear()
    {
        lock (_lock)
        {
            _list.Clear();
        }
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _list.Add(item);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _list.AddRange(items);
        }
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _list.Clear();
            _list.AddRange(items);
        }
    }

    /// <summary>
    /// Returns a snapshot copy of the list to allow safe enumeration without holding the lock.
    /// </summary>
    public List<T> ToList()
    {
        lock (_lock)
        {
            return [.. _list];
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a thread-safe snapshot copy of the list.
    /// This allows LINQ expressions and foreach loops to run safely without throwing exceptions
    /// if the collection is altered mid-loop by another thread.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        List<T> snapshot;
        lock (_lock)
        {
            snapshot = [.. _list];
        }

        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void RemoveAt(int idx)
    {
        lock (_lock)
        {
            _list.RemoveAt(idx);
        }
    }

    private T Get(int idx)
    {
        lock (_lock)
        {
            return _list[idx];
        }
    }

    public long Count
    {
        get
        {
            lock (_lock)
            {
                return _list.Count;
            }
        }
    }
}