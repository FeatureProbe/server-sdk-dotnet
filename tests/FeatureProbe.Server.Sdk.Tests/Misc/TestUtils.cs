using System.Reflection;

namespace FeatureProbe.Server.Sdk.UT;

internal static class TestUtils
{
    // https://gist.github.com/xpl0t/0d223222696a1c92d7d23cf8368800bf
    public static T Invoke<T>(this object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType()
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            throw new ArgumentException($"No private method \"{methodName}\" found in class \"{obj.GetType().Name}\"");
        }

        var res = method.Invoke(obj, parameters);
        if (res is T)
        {
            return (T)res;
        }

        throw new ArgumentException(
            $"Bad type parameter. Type parameter is of type \"{typeof(T).Name}\", whereas method invocation result is of type \"{res.GetType().Name}\"");
    }
}
