using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Models;

public class Segment
{
    [JsonPropertyName("uniqueId")] public string UniqueId { get; init; }

    [JsonPropertyName("version")] public long Version { get; init; }

    [JsonPropertyName("rules")] public List<SegmentRule> Rules { get; init; }

    public bool Contains(FPUser user, ImmutableDictionary<string, Segment> segments)
    {
        return Rules
            .Select(rule => rule.Hit(user, segments))
            .Any(hitResult => hitResult.Hit);
    }
}
