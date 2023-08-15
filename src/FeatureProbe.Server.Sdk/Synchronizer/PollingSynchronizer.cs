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

using System.Text.Json;
using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Synchronizer;

public class PollingSynchronizer : ISynchronizer
{
    private readonly string _apiUrl;

    private readonly IDataRepository _dataRepo;

    private readonly HttpClient _httpClient;
    private readonly TimeSpan _refreshInterval;

    private TaskCompletionSource<bool>? _initTask;

    private Timer? _timer;

    internal PollingSynchronizer(FPConfig config, IDataRepository dataRepo)
    {
        _refreshInterval = config.RefreshInterval;
        _apiUrl = config.SynchronizerUrl;
        _dataRepo = dataRepo;

        _httpClient = new HttpClient { Timeout = config.HttpConfig.RequestTimeout };
        foreach (var header in config.HttpConfig.Headers)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        _timer = null;
        _initTask = new TaskCompletionSource<bool>();
    }

    public async Task SynchronizeAsync()
    {
        Loggers.Synchronizer?.Log(
            LogLevel.Information,
            "Starting FeatureProbe polling repository with interval {0} ms", _refreshInterval.TotalMilliseconds
        );
        lock (this)
        {
            _timer ??= new Timer(tcs =>
            {
                PollAsync().Wait();
                ((TaskCompletionSource<bool>?)tcs)?.TrySetResult(true);
            }, _initTask, TimeSpan.Zero, _refreshInterval);
        }

        if (_initTask is not null)
        {
            await _initTask.Task;
            _initTask = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Loggers.Synchronizer?.Log(LogLevel.Information, "Closing FeatureProbe PollingSynchronizer");
        lock (this)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    public async Task PollAsync()
    {
        try
        {
            using var resp = await _httpClient.GetAsync(_apiUrl);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStreamAsync();
            var repository = await JsonSerializer.DeserializeAsync<Repository>(json);

            Loggers.Synchronizer?.Log(LogLevel.Debug,
                "Http response body: {0}", JsonSerializer.Serialize(repository));

            _dataRepo.Refresh(repository);
        }
        catch (Exception e)
        {
            Loggers.Synchronizer?.Log(LogLevel.Error, e, "Unexpected error from polling processor");
        }
    }
}

public class PollingSynchronizerFactory : ISynchronizerFactory
{
    public ISynchronizer Create(FPConfig config, IDataRepository dataRepo)
    {
        return new PollingSynchronizer(config, dataRepo);
    }
}
