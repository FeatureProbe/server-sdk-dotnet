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
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Predicates;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Models;

public class Condition
{
    private readonly IMatcher _matcher;

    [JsonConstructor]
    public Condition(string type, string predicate)
    {
        Type = type;
        Predicate = predicate;
        _matcher = PredicateUtils.GetMatcher(type, predicate);
    }

    [JsonPropertyName("subject")] public string Subject { get; init; }

    [JsonPropertyName("type")] public string Type { get; init; }

    [JsonPropertyName("predicate")] public string Predicate { get; init; }

    [JsonPropertyName("objects")] public List<string> Objects { get; set; }

    public bool MatchObjects(FPUser user, ImmutableDictionary<string, Segment> segments)
    {
        try
        {
            return _matcher.Match(new MatchContext
            {
                Subject = Subject, Objects = Objects, User = user, Segments = segments
            });
        }
        catch (Exception e)
        {
            Loggers.Main?.LogError(e, "Error while matching objects");
            return false;
        }
    }
}
