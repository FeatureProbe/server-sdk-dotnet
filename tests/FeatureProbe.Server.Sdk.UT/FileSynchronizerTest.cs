using System.Reflection;
using FeatureProbe.Server.Sdk.DataRepositories;

namespace FeatureProbe.Server.Sdk.UT;

public class FileSynchronizerTest
{
    [Fact]
    void TestDeserializeFile()
    {
        var config = new FPConfig.Builder()
            .ServerSdkKey("server-8ed48815ef044428826787e9a238b9c6a479f98c")
            .LocalFileMode(Path.Combine(Environment.CurrentDirectory, "resources/datasource/repo.json"))
            .Build();

        using var fp = new FPClient(config, 100);
        var dataRepo = (IDataRepository)fp.GetType()
            .GetField("_dataRepository", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fp)!;

        Assert.True(dataRepo.Initialized);
        Assert.True(dataRepo.Segments.Count > 0);
        Assert.True(dataRepo.Toggles.Count > 0);
    }
}
