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

using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Events;

public record AccessEvent(
    string User,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] object? Value,
    [property: JsonPropertyName("version")]
    long? Version,
    [property: JsonPropertyName("variationIndex")]
    int? VariationIndex,
    [property: JsonPropertyName("ruleIndex")]
    int? RuleIndex,
    [property: JsonIgnore] bool TrackAccessEvents
) : BaseEvent(
    "access",
    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
    User
);
