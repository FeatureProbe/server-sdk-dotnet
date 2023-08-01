using System.Text.Json.Serialization;

namespace FeatureProbe.Server.Sdk.Models;

public class Prerequisite
{
    [JsonPropertyName("key")] public string Key { get; set; }

    [JsonPropertyName("value")] public string Value { get; set; }
}
