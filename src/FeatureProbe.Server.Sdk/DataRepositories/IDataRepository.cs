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

using System.Collections.Immutable;
using FeatureProbe.Server.Sdk.Models;

namespace FeatureProbe.Server.Sdk.DataRepositories;

public interface IDataRepository : IAsyncDisposable
{
    ImmutableDictionary<string, Toggle> Toggles { get; }

    ImmutableDictionary<string, Segment> Segments { get; }

    long DebugUntilTime { get; }

    bool Initialized { get; }

    Toggle? GetToggle(string key);

    Segment? GetSegment(string key);

    void Refresh(Repository? repo);
}

public interface IDataRepositoryFactory
{
    IDataRepository Create(FPConfig config);
}
