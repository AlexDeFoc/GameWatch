using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GameWatch.Core;

public class ConcurrentList<T> : IEnumerable<T>
{
    private readonly List<T> _list = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the number of elements contained in the list atomically.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _list.Count;
            }
        }
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _list.Add(item);
        }
    }

    /// <summary>
    /// Appends all items from the specified collection to the end of the list atomically.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _list.AddRange(items);
        }
    }

    /// <summary>
    /// Clears the existing list and populates it with the contents of the specified collection atomically.
    /// </summary>
    public void ReplaceAll(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _list.Clear();
            _list.AddRange(items);
        }
    }

    public T Get(int index)
    {
        lock (_lock)
        {
            return _list[index];
        }
    }

    /// <summary>
    /// Returns a snapshot copy of the list to allow safe enumeration without holding the lock.
    /// </summary>
    public List<T> ToList()
    {
        lock (_lock)
        {
            return [.._list];
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
            snapshot = new List<T>(_list);
        }
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}