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
using FeatureProbe.Server.Sdk.Models;
using Moq;

namespace FeatureProbe.Server.Sdk.IT;

public class StreamingSynchronizerIT
{
    // TODO: use TestContainers

    internal static readonly Mock<MemoryDataRepository> _mockedDataRepository = new();

    // temporary disabled as `https://featureprobe.io/server` is not available now
    // [Fact]
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
