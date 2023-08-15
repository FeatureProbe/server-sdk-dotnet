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
