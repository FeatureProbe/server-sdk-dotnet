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
