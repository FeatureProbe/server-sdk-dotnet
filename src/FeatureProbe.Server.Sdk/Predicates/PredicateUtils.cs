using System.Reflection;
using FeatureProbe.Server.Sdk.Internal;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Predicates;

public static class PredicateUtils
{
    private static readonly Dictionary<string, Dictionary<string, IMatcher>> Matchers;

    static PredicateUtils()
    {
        Matchers = new();

        var matchers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsDefined(typeof(MatcherFor)));

        foreach (var matcher in matchers)
        {
            if (Activator.CreateInstance(matcher) is IMatcher matcherInst)
            {
                var attr = (MatcherFor)matcher.GetCustomAttribute(typeof(MatcherFor))!;
                if (!Matchers.TryGetValue(attr.ConditionType, out var res))
                {
                    res = new();
                    Matchers[attr.ConditionType] = res;
                }

                res[attr.Predicate] = matcherInst;
            }
        }
    }

    private class DummyMatcher : IMatcher
    {
        public bool Match(MatchContext ctx)
        {
            return false;
        }
    }

    public static IMatcher GetMatcher(string conditionType, string predicate)
    {
        try
        {
            return Matchers[conditionType][predicate];
        }
        catch (KeyNotFoundException e)
        {
            Loggers.Evaluator?.Log(LogLevel.Error, e,
                "Invalid condition type ({Type}) & predicate ({Predicate}), matcher will always return false",
                conditionType, predicate);
            return new DummyMatcher();
        }
    }
}
