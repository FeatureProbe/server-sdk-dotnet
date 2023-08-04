using FeatureProbe.Server.Sdk.Events;
using FeatureProbe.Server.Sdk.Processors;

namespace FeatureProbe.Server.Sdk.UT;

public class AccessSummaryRecorderTest
{
    private readonly AccessEvent _event;
    
    private readonly AccessSummaryRecorder _recorder = new();

    AccessSummaryRecorderTest()
    {
        var user = new FPUser().StableRollout("test_user");

        _event = new AccessEvent(
            User: user.Key,
            Key: "test_toggle",
            Value: "true",
            Version: 1,
            VariationIndex: 0,
            RuleIndex: 1,
            TrackAccessEvents: true
        );
    }

    [Fact]
    void TestAddEvent()
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
}
