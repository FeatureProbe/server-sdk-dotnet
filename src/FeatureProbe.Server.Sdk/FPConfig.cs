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

using System.Reflection;
using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Processors;
using FeatureProbe.Server.Sdk.Synchronizer;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk;

public sealed class FPConfig
{
    private FPConfig(Builder builder)
    {
        ServerSdkKey = string.IsNullOrWhiteSpace(builder.ServerSdkKeyVal)
            ? throw new ArgumentException("ServerSdkKey is required and must not be blank")
            : builder.ServerSdkKeyVal;
        DataRepositoryFactory = builder.DataRepositoryFactory ?? new MemoryDataRepositoryFactory();
        SynchronizerFactory = builder.SynchronizerFactory ?? new PollingSynchronizerFactory();
        EventProcessorFactory = new DefaultEventProcessorFactory();
        RemoteUrl = builder.RemoteUrlVal ?? "http://localhost:4009/server";
        SynchronizerUrl = builder.SynchronizerUrlVal ?? $"{RemoteUrl}/api/server-sdk/toggles";
        EventUrl = builder.EventUrlVal ?? $"{RemoteUrl}/api/events";
        RealtimeUrl = builder.RealtimeUrlVal ?? $"{RemoteUrl}/realtime";
        FileLocation = builder.FileLocation ?? Path.Combine("datasource", "repo.json");
        RefreshInterval = builder.RefreshInterval ?? TimeSpan.FromSeconds(5);
        HttpConfig = builder.HttpConfig ?? new HttpConfig();
        PrerequisiteDeep = builder.PrerequisiteDeepVal ?? 20;

        var version = Assembly.GetAssembly(typeof(FPClient))?.GetName().Version?.ToString(3) ?? "unknown";
        HttpConfig.Headers.Add(new KeyValuePair<string, string>("Authorization", ServerSdkKey));
        HttpConfig.Headers.Add(new KeyValuePair<string, string>("user-agent", $"DotNet/{version}"));
    }

    internal string ServerSdkKey { get; }

    internal IDataRepositoryFactory DataRepositoryFactory { get; }

    internal ISynchronizerFactory SynchronizerFactory { get; }

    internal IEventProcessorFactory EventProcessorFactory { get; }

    internal string RemoteUrl { get; }

    internal string SynchronizerUrl { get; }

    internal string EventUrl { get; }

    internal string RealtimeUrl { get; }

    internal string FileLocation { get; }

    internal TimeSpan RefreshInterval { get; }

    internal HttpConfig HttpConfig { get; }

    internal int PrerequisiteDeep { get; }

    public class Builder
    {
        internal string? ServerSdkKeyVal { get; private set; }

        internal IDataRepositoryFactory? DataRepositoryFactory { get; private set; }

        internal ISynchronizerFactory? SynchronizerFactory { get; private set; }

        internal IEventProcessorFactory? EventProcessorFactory { get; }

        internal string? RemoteUrlVal { get; private set; }

        internal string? SynchronizerUrlVal { get; private set; }

        internal string? EventUrlVal { get; private set; }

        internal string? RealtimeUrlVal { get; private set; }

        internal string? FileLocation { get; private set; }

        internal TimeSpan? RefreshInterval { get; private set; }

        internal HttpConfig? HttpConfig { get; private set; }

        internal int? PrerequisiteDeepVal { get; private set; }

        /// <summary>
        ///     Set SDK key for your FeatureProbe environment.
        /// </summary>
        public Builder ServerSdkKey(string serverSdkKey)
        {
            ServerSdkKeyVal = serverSdkKey;
            return this;
        }

        /// <summary>
        ///     Will synchronize segments, toggles, etc. from your FeatureProbe server.
        /// </summary>
        /// <param name="refreshInterval">Duration between two polls, default is 5 sec</param>
        /// <param name="httpConfig">Configuration for HttpClient</param>
        public Builder PollingMode(TimeSpan? refreshInterval = null, HttpConfig? httpConfig = null)
        {
            RefreshInterval = refreshInterval ?? TimeSpan.FromSeconds(5);
            HttpConfig = httpConfig;
            SynchronizerFactory = new PollingSynchronizerFactory();
            return this;
        }

        /// <summary>
        ///     Use a polling synchronizer and an additional websocket to update segments, toggles, etc. in realtime.
        /// </summary>
        /// <param name="refreshInterval">Duration between two polls, default is 10 sec</param>
        /// <param name="httpConfig">Configuration for HttpClient</param>
        /// <returns></returns>
        public Builder StreamingMode(TimeSpan? refreshInterval = null, HttpConfig? httpConfig = null)
        {
            RefreshInterval = refreshInterval ?? TimeSpan.FromSeconds(10);
            HttpConfig = httpConfig;
            SynchronizerFactory = new StreamingSynchronizerFactory();
            return this;
        }

        /// <summary>
        ///     URL for FeatureProbe server, default is a local server, i.e. http://localhost:4009/server.
        /// </summary>
        public Builder RemoteUrl(string url)
        {
            RemoteUrlVal = url;
            return this;
        }

        /// <summary>
        ///     Overwrite the URL for synchronizer.
        /// </summary>
        public Builder SynchronizerUrl(string url)
        {
            SynchronizerUrlVal = url;
            return this;
        }

        /// <summary>
        ///     Overwrite the URL for event reporting.
        /// </summary>
        public Builder EventUrl(string url)
        {
            EventUrlVal = url;
            return this;
        }

        /// <summary>
        ///     Override the URL for websocket connection.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Builder RealtimeUrl(string url)
        {
            RealtimeUrlVal = url;
            return this;
        }

        /// <summary>
        ///     Will synchronize segments, toggles, etc. from a local file.
        /// </summary>
        /// <param name="location">
        ///     File path, default will be 'datasource/repo.json' under current directory.
        /// </param>
        public Builder LocalFileMode(string? location)
        {
            FileLocation = location ?? Path.Combine("datasource", "repo.json");
            SynchronizerFactory = new FileSynchronizerFactory();
            return this;
        }

        /// <summary>
        ///     [Default] Store repository in memory.
        /// </summary>
        public Builder UseMemoryDataRepository()
        {
            DataRepositoryFactory = new MemoryDataRepositoryFactory();
            return this;
        }

        /// <summary>
        ///     Restrict the depth of prerequisite evaluation.
        /// </summary>
        /// <param name="deep">max depth for prerequisite</param>
        public Builder PrerequisiteDeep(int deep)
        {
            PrerequisiteDeepVal = deep;
            return this;
        }

        /// <summary>
        ///     Provide your logger configuration to enable logging.
        /// </summary>
        public Builder WithLoggers(ILoggerFactory loggerFactory)
        {
            Loggers.Factory = loggerFactory;
            return this;
        }

        public FPConfig Build()
        {
            return new FPConfig(this);
        }
    }
}

public sealed class HttpConfig
{
    /// <summary>
    ///     Timeout for a HTTP request
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Any additional headers you want to add for the request
    /// </summary>
    public List<KeyValuePair<string, string>> Headers { get; } = new();
}
