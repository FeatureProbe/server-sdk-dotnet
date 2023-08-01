using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Internal;

namespace FeatureProbe.Server.Sdk.Synchronizer;

public interface ISynchronizer : IAsyncDisposable
{
    Task SynchronizeAsync();
}

public interface ISynchronizerFactory
{
    ISynchronizer Create(FPConfig config, IDataRepository dataRepo);
}
