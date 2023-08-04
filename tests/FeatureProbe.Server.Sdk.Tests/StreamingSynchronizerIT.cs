using System.Reflection;
using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Models;
using Moq;

namespace FeatureProbe.Server.Sdk.IT;

public class StreamingSynchronizerIT
{
    // TODO: use TestContainers

    internal static readonly Mock<MemoryDataRepository> _mockedDataRepository = new();

    [Fact]
    private void TestSocketRealtimeToggleUpdate()
    {
        var config = new FPConfig.Builder()
            .ServerSdkKey("server-8ed48815ef044428826787e9a238b9c6a479f98c")
            .RemoteUrl("https://featureprobe.io/server")
            .StreamingMode()
            .UseMockedDataRepository()
            .Build();

        var fpClient = new FPClient(config, -1);
        Thread.Sleep(5000);
        fpClient.Dispose();

        // polling start + socket update event
        _mockedDataRepository.Verify(
            x => x.Refresh(It.IsAny<Repository?>()),
            Times.AtLeast(2)
        );
    }
}

internal static class MockSpec
{
    public static FPConfig.Builder UseMockedDataRepository(this FPConfig.Builder builder)
    {
        var dataRepoFactoryField = builder.GetType()
            .GetProperty("DataRepositoryFactory", BindingFlags.NonPublic | BindingFlags.Instance)!;
        dataRepoFactoryField.SetValue(builder, new MockedDataRepositoryFactory(),
            BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

        return builder;
    }

    private class MockedDataRepositoryFactory : IDataRepositoryFactory
    {
        public IDataRepository Create(FPConfig config)
        {
            return StreamingSynchronizerIT._mockedDataRepository.Object;
        }
    }
}
