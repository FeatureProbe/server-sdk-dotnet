/*
 * Copyright 2023 FeatureProbe
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Reflection;
using FeatureProbe.Server.Sdk.Internal;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Predicates;

public static class PredicateUtils
{
    private static readonly Dictionary<string, Dictionary<string, IMatcher>> Matchers;

    static PredicateUtils()
    {
        Matchers = new Dictionary<string, Dictionary<string, IMatcher>>();

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
                    res = new Dictionary<string, IMatcher>();
                    Matchers[attr.ConditionType] = res;
                }

                res[attr.Predicate] = matcherInst;
            }
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

    private class DummyMatcher : IMatcher
    {
        public bool Match(MatchContext ctx)
        {
            return false;
        }
    }
}
