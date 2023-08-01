using System.Collections.Immutable;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.DataRepositories;

public class MemoryDataRepository : IDataRepository
{
    private volatile Repository? _data;

    private volatile bool _initialized = false;

    private long _updatedTimestamp = 0;

    public ImmutableDictionary<string, Toggle> Toggles
    {
        get
        {
            if (!_initialized) return ImmutableDictionary<string, Toggle>.Empty;
            return _data?.Toggles ?? ImmutableDictionary<string, Toggle>.Empty;
        }
    }

    public ImmutableDictionary<string, Segment> Segments
    {
        get
        {
            if (!_initialized) return ImmutableDictionary<string, Segment>.Empty;
            return _data?.Segments ?? ImmutableDictionary<string, Segment>.Empty;
        }
    }

    public long DebugUntilTime
    {
        get
        {
            return _data?.DebugUntilTime ?? 0;
        }
    }

    public bool Initialized
    {
        get
        {
            return _initialized;
        }
    }

    public Toggle? GetToggle(string key)
    {
        return !_initialized ? null : _data?.Toggles.TryGetValue(key);
    }

    public Segment? GetSegment(string key)
    {
        return !_initialized ? null : _data?.Segments.TryGetValue(key);
    }

    public void Refresh(Repository? repo)
    {
        if (repo?.Segments is null || repo?.Toggles is null) return;
        lock (this)
        {
            _data = new Repository
            {
                Segments = repo.Segments, Toggles = repo.Toggles, DebugUntilTime = repo.DebugUntilTime
            };
            _initialized = true;
            _updatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _data = null;
        _initialized = false;
        _updatedTimestamp = 0;
    }
}

public class MemoryDataRepositoryFactory : IDataRepositoryFactory
{
    public IDataRepository Create(FPConfig config)
    {
        return new MemoryDataRepository();
    }
}
