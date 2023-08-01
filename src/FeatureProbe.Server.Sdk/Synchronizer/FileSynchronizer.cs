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
