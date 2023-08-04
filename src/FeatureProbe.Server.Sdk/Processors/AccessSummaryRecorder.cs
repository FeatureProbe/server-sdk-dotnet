using System.Collections;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Events;

namespace FeatureProbe.Server.Sdk.Processors;

public class AccessSummaryRecorder
{
    [JsonPropertyName("counters")]
    public Dictionary<string, List<AccessCounter>> Counters { get; private set; } = new();

    [JsonPropertyName("startTime")]
    public long StartTime { get; private set; }

    [JsonPropertyName("endTime")]
    public long EndTime { get; private set; }

    public void Add(AccessEvent @event)
    {
        if (Counters.Count == 0) StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (!Counters.ContainsKey(@event.Key))
        {
            var groups = new List<AccessCounter> { new(@event.Value, @event.Version, @event.VariationIndex) };
            Counters.Add(@event.Key, groups);
        }
        else
        {
            var counters = Counters[@event.Key];
            var existingCnt = counters.Find(c => c.IsGroup(@event.Version, @event.VariationIndex));
            
            if (existingCnt is not null) existingCnt.Increment();
            else counters.Add(new AccessCounter(@event.Value, @event.Version, @event.VariationIndex));
        }
    }

    public AccessSummaryRecorder Snapshot()
    {
        return new AccessSummaryRecorder
        {
            Counters = this.Counters.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(counter => (AccessCounter)counter.Clone()).ToList()
            ),
            StartTime = this.StartTime,
            EndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }

    public void Clear() => this.Counters = new();
}

public class AccessCounter : ICloneable
{
    [JsonPropertyName("count")]
    public long Count { get; private set; }

    [JsonPropertyName("value")]
    public object? Value { get; }

    [JsonPropertyName("version")]
    public long? Version { get; }

    [JsonPropertyName("index")]
    public int? Index { get; }

    public AccessCounter(object? value, long? version, int? index)
    {
        this.Count = 1;
        this.Value = value;
        this.Version = version;
        this.Index = index;
    }

    public void Increment()
    {
        this.Count++;
    }

    public bool IsGroup(long? version, int? index)
    {
        return this.Version == version && this.Index == index;
    }

    public object Clone()
    {
        return new AccessCounter(this.Value, this.Version, this.Index) { Count = this.Count };
    }
}
