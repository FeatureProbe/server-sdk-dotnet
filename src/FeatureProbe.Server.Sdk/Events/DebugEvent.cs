using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Events;

public record DebugEvent(
    [property: JsonPropertyName("userDetail")]
    FPUser UserDetail,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] object? Value,
    [property: JsonPropertyName("version")]
    long? Version,
    [property: JsonPropertyName("variationIndex")]
    int? VariationIndex,
    [property: JsonPropertyName("ruleIndex")]
    int? RuleIndex,
    [property: JsonPropertyName("reason")] string? Reason
) : BaseEvent(
    Kind: "debug",
    Time: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
    User: UserDetail.Key
);
