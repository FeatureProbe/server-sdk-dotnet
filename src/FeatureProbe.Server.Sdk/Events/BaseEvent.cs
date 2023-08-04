using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Events;

[JsonDerivedType(typeof(AccessEvent))]
[JsonDerivedType(typeof(CustomEvent))]
[JsonDerivedType(typeof(DebugEvent))]
public record BaseEvent(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("user")] string User
);
