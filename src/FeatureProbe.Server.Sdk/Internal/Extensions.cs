namespace FeatureProbe.Server.Sdk.Internal;

public static class Extensions
{
    public static V? TryGetValue<K, V>(this IDictionary<K, V> dict, K key)
    {
        dict.TryGetValue(key, out var value);
        return value;
    }
}
