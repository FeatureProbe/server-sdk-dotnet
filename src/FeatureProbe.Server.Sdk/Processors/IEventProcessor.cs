using FeatureProbe.Server.Sdk.Events;
using FeatureProbe.Server.Sdk.Internal;

namespace FeatureProbe.Server.Sdk.Processors;

public interface IEventProcessor
{
    void Push(BaseEvent @event);

    void Flush();
    
    Task ShutdownAsync();
}

public interface IEventProcessorFactory
{
    IEventProcessor Create(FPConfig config);
}
