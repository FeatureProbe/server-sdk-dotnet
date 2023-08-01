using System.Collections.Immutable;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class SegmentRule
{
    [JsonPropertyName("conditions")] public List<Condition> Conditions { get; init; }

    public HitResult Hit(FPUser user, ImmutableDictionary<string, Segment> segments)
    {
        foreach (var condition in Conditions)
        {
            if (!"segment".Equals(condition.Type)
                && !user.ContainAttr(condition.Subject))
            {
                return new HitResult(false,
                    Reason: $"Warning: User with key '{user.Key}' does not have attribute name '{condition.Subject}'");
            }

            if (!condition.MatchObjects(user, segments))
            {
                return new HitResult(false);
            }
        }

        return new HitResult(true);
    }
}
