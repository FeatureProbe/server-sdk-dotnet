using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Predicates;

namespace FeatureProbe.Server.Sdk.Models;

public class Condition
{
    [JsonPropertyName("subject")] public string Subject { get; init; }

    [JsonPropertyName("type")] public string Type { get; init; }

    [JsonPropertyName("predicate")] public string Predicate { get; init; }

    [JsonPropertyName("objects")] public List<string> Objects { get; set; }

    private readonly IMatcher _matcher;

    [JsonConstructor]
    public Condition(string type, string predicate)
    {
        Type = type;
        Predicate = predicate;
        _matcher = PredicateUtils.GetMatcher(type, predicate);
    }

    public bool MatchObjects(FPUser user, ImmutableDictionary<string, Segment> segments)
    {
        try
        {
            return _matcher.Match(new MatchContext
            {
                Subject = this.Subject,
                Objects = this.Objects,
                User = user,
                Segments = segments,
            });
        }
        catch (Exception e)
        {
            Loggers.Main?.LogError(e, "Error while matching objects");
            return false;
        }
    }
}
