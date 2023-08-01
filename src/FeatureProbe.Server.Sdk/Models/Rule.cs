using System.Collections.Immutable;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Predicates;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class Rule
{
    [JsonPropertyName("serve")] public Serve Serve { get; init; }

    [JsonPropertyName("conditions")] public List<Condition> Conditions { get; init; }

    public HitResult Hit(FPUser? user, ImmutableDictionary<string, Segment> segments, string? toggleKey)
    {
        if (user is null || string.IsNullOrEmpty(toggleKey))
        {
            return new HitResult(false);
        }

        foreach (var condition in Conditions)
        {
            if (!"segment".Equals(condition.Type)
                && !"datetime".Equals(condition.Type)
                && !user.ContainAttr(condition.Subject))
            {
                return new HitResult(
                    Hit: false,
                    Reason: $"Warning: User with key '{user.Key}' does not have attribute name '{condition.Subject}'"
                );
            }

            if (!condition.MatchObjects(user, segments))
            {
                return new HitResult(false);
            }
        }

        return Serve.EvalIndex(user, toggleKey);
    }
}
