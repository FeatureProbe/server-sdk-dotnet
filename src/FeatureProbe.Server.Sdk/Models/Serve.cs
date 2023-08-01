using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class Serve
{
    [JsonPropertyName("select")] public int? Select { get; set; }

    [JsonPropertyName("split")] public Split Split { get; set; }

    public HitResult EvalIndex(FPUser user, String toggleKey)
    {
        if (Select is not null)
        {
            return new HitResult(true, Index: Select);
        }

        return Split.FindIndex(user, toggleKey);
    }
}
