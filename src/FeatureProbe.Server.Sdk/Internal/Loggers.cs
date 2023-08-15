/*
 * Copyright 2023 FeatureProbe
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
