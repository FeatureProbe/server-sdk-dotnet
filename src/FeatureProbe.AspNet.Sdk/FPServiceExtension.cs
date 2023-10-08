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

﻿using FeatureProbe.Server.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.AspNet.Sdk;

public static class FPServiceExtension
{
    public static IServiceCollection AddFeatureProbe(this IServiceCollection services, IConfigurationSection config)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var fpConfig = new FPConfig.Builder()
            .WithLoggers(services.BuildServiceProvider().GetRequiredService<ILoggerFactory>())
            .ServerSdkKey(config["SdkKey"] ?? throw new ArgumentNullException("SdkKey"))
            .RemoteUrl(config["RemoteUrl"])
            .EventUrl(config["EventUrl"])
            .SynchronizerUrl(config["SynchronizerUrl"])
            .RealtimeUrl(config["RealtimeUrl"])
            .StreamingMode(refreshInterval: TimeSpan.FromSeconds(Int32.Parse(config["RefreshInterval"] ?? "5")))
            .UseMemoryDataRepository()
            .Build();

        var fpClient = new FPClient(fpConfig, Int32.Parse(config["StartWait"] ?? "500"));
        services.AddSingleton(fpClient);

        var lifetime = services.BuildServiceProvider().GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(fpClient.Dispose);

        return services;
    }
}
