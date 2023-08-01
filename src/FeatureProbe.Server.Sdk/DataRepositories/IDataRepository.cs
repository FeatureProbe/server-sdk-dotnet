using System.Collections.Immutable;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.DataRepositories;

public interface IDataRepository : IAsyncDisposable
{
    ImmutableDictionary<string, Toggle> Toggles { get; }

    ImmutableDictionary<string, Segment> Segments { get; }

    long DebugUntilTime { get; }

    bool Initialized { get; }

    Toggle? GetToggle(string key);

    Segment? GetSegment(string key);
    
    void Refresh(Repository? repo);
}

public interface IDataRepositoryFactory
{
    IDataRepository Create(FPConfig config);
}
