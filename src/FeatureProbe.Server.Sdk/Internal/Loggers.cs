using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Internal;

internal static class Loggers
{
    public static ILoggerFactory? Factory
    {
        set
        {
            Main = value?.CreateLogger("FeatureProbe");
            Synchronizer = value?.CreateLogger("FeatureProbe-Synchronizer");
            Event = value?.CreateLogger("FeatureProbe-Event");
            Evaluator = value?.CreateLogger("FeatureProbe-Evaluator");
        }
    }

    public static ILogger? Main { get; private set; }

    public static ILogger? Synchronizer { get; private set; }

    public static ILogger? Event { get; private set; }

    public static ILogger? Evaluator { get; private set; }
}
