using System.Collections.Immutable;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.Predicates;

public interface IMatcher
{
    bool Match(MatchContext ctx);
}

public readonly record struct MatchContext
(
    string Subject,
    List<string> Objects,
    FPUser User,
    ImmutableDictionary<string, Segment> Segments
);

[AttributeUsage(AttributeTargets.Class)]
public class MatcherFor : Attribute
{
    public MatcherFor(string conditionType, string predicate)
    {
        ConditionType = conditionType;
        Predicate = predicate;
    }

    public string ConditionType { get; }

    public string Predicate { get; }
}
