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
