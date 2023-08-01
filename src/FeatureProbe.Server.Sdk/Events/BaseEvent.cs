using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Events;

public record BaseEvent(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("user")] string User
);
