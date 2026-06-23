using System.Collections.Concurrent;

namespace ArtClass.Application.Caching;

public interface IAppDataCache
{
    long Version { get; }

    bool TryGet<T>(string key, out T? value);

    void Set<T>(string key, T value);

    void InvalidateSchedule();

    void InvalidateGroups();

    void InvalidateStudents();

    void InvalidateAll();
}

public sealed class AppDataCache : IAppDataCache
{
    private readonly ConcurrentDictionary<string, object> _entries = new();
    private long _version;

    public long Version => Volatile.Read(ref _version);

    private void BumpVersion() => Interlocked.Increment(ref _version);

    public bool TryGet<T>(string key, out T? value)
    {
        if (_entries.TryGetValue(key, out var cached) && cached is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value) =>
        _entries[key] = value!;

    public void InvalidateSchedule()
    {
        foreach (var key in _entries.Keys)
        {
            if (key.StartsWith("calendar:", StringComparison.Ordinal)
                || key.StartsWith("day:", StringComparison.Ordinal)
                || key.StartsWith("cycle", StringComparison.Ordinal))
            {
                _entries.TryRemove(key, out _);
            }
        }

        _entries.TryRemove("calendar:repeating-slots", out _);
        BumpVersion();
    }

    public void InvalidateGroups()
    {
        foreach (var key in _entries.Keys)
        {
            if (key.StartsWith("groups", StringComparison.Ordinal)
                || key.StartsWith("group:", StringComparison.Ordinal))
            {
                _entries.TryRemove(key, out _);
            }
        }

        InvalidateSchedule();
    }

    public void InvalidateStudents()
    {
        foreach (var key in _entries.Keys)
        {
            if (key.StartsWith("students", StringComparison.Ordinal)
                || key.StartsWith("student:", StringComparison.Ordinal))
            {
                _entries.TryRemove(key, out _);
            }
        }

        InvalidateGroups();
    }

    public void InvalidateAll()
    {
        _entries.Clear();
        BumpVersion();
    }
}
