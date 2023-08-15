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

public class FileSynchronizer : ISynchronizer
{
    private readonly IDataRepository _dataRepo;
    private readonly string _filePath;

    internal FileSynchronizer(IDataRepository dataRepo, string filePath)
    {
        _dataRepo = dataRepo;
        _filePath = filePath;
    }

    public async Task SynchronizeAsync()
    {
        try
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            var repository = await JsonSerializer.DeserializeAsync<Repository>(fs);
            _dataRepo.Refresh(repository);
        }
        catch (IOException e)
        {
            Loggers.Synchronizer?.Log(
                LogLevel.Error, e,
                "Repository file resource not found in path: {_filePath}", _filePath
            );
        }
        catch (JsonException e)
        {
            Loggers.Synchronizer?.Log(
                LogLevel.Error, e,
                "Bad Repository JSON format in file: {_filePath}", _filePath
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Nothing to dispose
    }
}

public class FileSynchronizerFactory : ISynchronizerFactory
{
    public ISynchronizer Create(FPConfig config, IDataRepository dataRepo)
    {
        return new FileSynchronizer(dataRepo, config.FileLocation);
    }
}
