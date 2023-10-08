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
using Xunit.Abstractions;

namespace FeatureProbe.Server.Sdk.UT;

public class FileSynchronizerTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FileSynchronizerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    private void TestDeserializeFile()
    {
        var path = Path.Combine("resources", "datasource", "repo.json");
        var config = new FPConfig.Builder()
            .ServerSdkKey("server-8ed48815ef044428826787e9a238b9c6a479f98c")
            .LocalFileMode(path)
            .Build();

        using var fp = new FPClient(config, -1);
        var dataRepo = (IDataRepository)fp.GetType()
            .GetField("_dataRepository", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fp)!;

        Assert.True(dataRepo.Initialized);
        Assert.NotEmpty(dataRepo.Segments);
        Assert.NotEmpty(dataRepo.Toggles);
    }

    [Fact]
    private void TestDeserializeFileFail()
    {
        var path = Path.Combine("resources", "datasource", "not-exists.json");
        var config = new FPConfig.Builder()
            .ServerSdkKey("server-8ed48815ef044428826787e9a238b9c6a479f98c")
            .LocalFileMode(path)
            .Build();

        using var fp = new FPClient(config, -1);
        var dataRepo = (IDataRepository)fp.GetType()
            .GetField("_dataRepository", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fp)!;

        Assert.False(dataRepo.Initialized);
        Assert.Empty(dataRepo.Segments);
        Assert.Empty(dataRepo.Toggles);
    }
}
