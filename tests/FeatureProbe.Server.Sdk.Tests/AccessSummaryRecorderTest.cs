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

using FeatureProbe.Server.Sdk.Events;
using FeatureProbe.Server.Sdk.Processors;

namespace FeatureProbe.Server.Sdk.UT;

public class AccessSummaryRecorderTest
{
    private readonly AccessEvent _event;

    private readonly AccessSummaryRecorder _recorder = new();

    public AccessSummaryRecorderTest()
    {
        var user = new FPUser().StableRollout("test_user");

        _event = new AccessEvent(
            user.Key,
            "test_toggle",
            "true",
            1,
            0,
            1,
            true
        );
    }

    [Fact]
    private void TestAddEvent()
    {
        _recorder.Add(_event);

        Assert.True(_recorder.StartTime > 0);
        Assert.True(_recorder.EndTime == 0);

        var @event = _recorder.Counters["test_toggle"][0];
        Assert.True("true".Equals(@event.Value));
        Assert.True(1 == @event.Count);
        Assert.True(1 == @event.Version);
        Assert.True(0 == @event.Index);
    }

    [Fact]
    private void TestGetSnapshot()
    {
        _recorder.Add(_event);
        var snapshot = _recorder.Snapshot();
        _recorder.Clear();

        Assert.True(_recorder.Counters.Count == 0);

        Assert.True(snapshot.StartTime == _recorder.StartTime);
        Assert.True(snapshot.EndTime > 0);
        Assert.True(snapshot.Counters.Count == 1);
        Assert.True(snapshot.Counters["test_toggle"].Count == 1);
    }
}
