namespace FeatureProbe.Server.Sdk.Internal;

public static class Extensions
{
    public static TV? TryGetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key)
    {
        dict.TryGetValue(key, out var value);
        return value;
    }
}
