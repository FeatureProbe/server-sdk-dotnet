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
using FeatureProbe.Server.Sdk.Events;

namespace FeatureProbe.Server.Sdk.Processors;

public class AccessSummaryRecorder
{
    [JsonPropertyName("counters")]
    public Dictionary<string, List<AccessCounter>> Counters { get; private set; } = new();

    [JsonPropertyName("startTime")] public long StartTime { get; private set; }

    [JsonPropertyName("endTime")] public long EndTime { get; private set; }

    public void Add(AccessEvent @event)
    {
        if (Counters.Count == 0)
        {
            StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        if (!Counters.ContainsKey(@event.Key))
        {
            var groups = new List<AccessCounter> { new(@event.Value, @event.Version, @event.VariationIndex) };
            Counters.Add(@event.Key, groups);
        }
        else
        {
            var counters = Counters[@event.Key];
            var existingCnt = counters.Find(c => c.IsGroup(@event.Version, @event.VariationIndex));

            if (existingCnt is not null)
            {
                existingCnt.Increment();
            }
            else
            {
                counters.Add(new AccessCounter(@event.Value, @event.Version, @event.VariationIndex));
            }
        }
    }

    public AccessSummaryRecorder Snapshot()
    {
        return new AccessSummaryRecorder
        {
            Counters = Counters.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(counter => (AccessCounter)counter.Clone()).ToList()
            ),
            StartTime = StartTime,
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }

    public void Clear()
    {
        Counters = new Dictionary<string, List<AccessCounter>>();
    }
}

public class AccessCounter : ICloneable
{
    public AccessCounter(object? value, long? version, int? index)
    {
        Count = 1;
        Value = value;
        Version = version;
        Index = index;
    }

    [JsonPropertyName("count")] public long Count { get; private set; }

    [JsonPropertyName("value")] public object? Value { get; }

    [JsonPropertyName("version")] public long? Version { get; }

    [JsonPropertyName("index")] public int? Index { get; }

    public object Clone()
    {
        return new AccessCounter(Value, Version, Index) { Count = Count };
    }

    public void Increment()
    {
        Count++;
    }

    public bool IsGroup(long? version, int? index)
    {
        return Version == version && Index == index;
    }
}
