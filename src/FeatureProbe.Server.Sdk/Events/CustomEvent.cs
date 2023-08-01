using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Events;

public record CustomEvent(
    string User,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] double? Value
) : BaseEvent(
    Kind: "custom",
    Time: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
    User: User
);
