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
    Kind: "access",
    Time: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
    User: User
);
