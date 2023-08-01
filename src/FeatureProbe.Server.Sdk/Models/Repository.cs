using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Models;

public class Repository
{
    [JsonPropertyName("segments")] public ImmutableDictionary<string, Segment> Segments { get; init; } = ImmutableDictionary<string, Segment>.Empty;

    [JsonPropertyName("toggles")] public ImmutableDictionary<string, Toggle> Toggles { get; init; } = ImmutableDictionary<string, Toggle>.Empty;

    [JsonPropertyName("debugUntilTime")] public long? DebugUntilTime { get; init; }
}
