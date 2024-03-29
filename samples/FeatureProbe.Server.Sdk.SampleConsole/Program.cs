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
using NLog.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.SampleConsole;

public class Program
{
    public static void Main()
    {
        var config = new FPConfig.Builder()
            //// Must provide the server-side SDK Key for your project and environment
            .ServerSdkKey("server-8ed48815ef044428826787e9a238b9c6a479f98c")
            //// RemoteUrl is where you deploy the FeatureProbe server, by default, SDK will use its API for reporting events, synchronizing toggles, and so
            .RemoteUrl("https://featureprobe.io/server") // FeatureProbe online demo
            // .RemoteUrl("http://localhost:4009/server")  // Default URL for local Docker installation, also the default value if unset
            //// Below are three modes of synchronizing toggles, you can choose one of them
            .StreamingMode()
            // .PollingMode()
            // .LocalFileMode("datasource/repo.json")
            //// Optionally provide a logger factory to enable logging
            .WithLoggers(LoggerFactory.Create(builder => builder.AddNLog()))
            .Build();

        // in this example, startWait=-1 is used to wait infinitely for the client initialization
        using var fp = new FPClient(config, -1);

        // create a user with autogenerated key
        var user = new FPUser
        {
            ["userId"] = "00001" // "userId" is an sample attribute that will be used in the following rule of toggle
        };


        //
        // Get toggle result for this user.
        //
        const string toggleKey = "bool_toggle";

        var isOpen = fp.BoolValue(toggleKey, user, false);
        Console.WriteLine($"Feature for this user is: {isOpen}");

        // detailed evaluation allows you to get more information about the evaluation result
        var isOpenDetail = fp.BoolDetail(toggleKey, user, false);
        Console.WriteLine($"Detail reason: {isOpenDetail.Reason}");
        Console.WriteLine($"Rule index: {isOpenDetail.RuleIndex}");

        //
        // Simulate conversion rate of 1000 users for a new feature.
        //
        var demoTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = new Random();

        const string eventName = "new_feature_conversion";

        for (var i = 0; i < 1000; i++)
        {
            var eventUser = new FPUser().StableRollout($@"dotnet-demo-{demoTimestamp}/{i.ToString()}");
            var newFeature = fp.BoolValue(toggleKey, eventUser, false);

            if (newFeature)
            {
                if (random.Next(100) <= 55)
                {
                    Console.WriteLine("New feature conversion.");
                    fp.Track(eventName, eventUser);
                }
            }
            else
            {
                if (random.Next(100) > 55)
                {
                    Console.WriteLine("Old feature conversion.");
                    fp.Track(eventName, eventUser);
                }
            }

            Thread.Sleep(200);
        }
    }
}
