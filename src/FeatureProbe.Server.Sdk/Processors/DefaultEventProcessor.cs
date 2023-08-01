using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Events;
using FeatureProbe.Server.Sdk.Internal;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk.Processors;

public class DefaultEventProcessor : IEventProcessor
{
    private const int EventQueueCapacity = 10000;

    private readonly FPConfig _config;

    private readonly BlockingCollection<EventAction> _eventQueue = new(EventQueueCapacity);

    private readonly EventRepository _eventRepo = new();

    private readonly Timer _flushTimer;

    private readonly Task _handleEventTask;

    private readonly List<EventAction> _handlingActions = new();

    private readonly HttpClient _httpClient;

    private readonly TaskFactory _taskFactory = new(TaskScheduler.Default);

    private readonly List<Task> _tasks = new();

    public DefaultEventProcessor(FPConfig config)
    {
        _config = config;

        _flushTimer = new Timer(_ => Flush(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        _httpClient = new HttpClient { Timeout = config.HttpConfig.RequestTimeout };
        foreach (var header in config.HttpConfig.Headers)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        _handleEventTask = _taskFactory.StartNew(HandleEventDaemon, TaskCreationOptions.LongRunning);
    }

    public bool Closed => _eventQueue.IsAddingCompleted;

    public void Push(BaseEvent @event)
    {
        if (Closed)
        {
            return;
        }

        if (!_eventQueue.TryAdd(new EventAction(EventActionType.Event, @event)))
        {
            Loggers.Event?.Log(LogLevel.Warning, "Event processing is busy, some will be dropped");
        }
    }

    public void Flush()
    {
        if (Closed)
        {
            return;
        }

        if (!_eventQueue.TryAdd(new EventAction(EventActionType.Flush, null)))
        {
            Loggers.Event?.Log(LogLevel.Warning, "Event processing is busy, some will be dropped");
        }
    }

    public async Task ShutdownAsync()
    {
        if (Closed)
        {
            return;
        }

        Flush();
        _eventQueue.CompleteAdding();

        try
        {
            // Error CS1061
            // ReSharper disable once MethodHasAsyncOverload
            _flushTimer.Dispose();

            await _handleEventTask;
            await Task.WhenAll(_tasks);
        }
        catch (Exception e)
        {
            Loggers.Event?.Log(LogLevel.Error, e, "FeatureProbe shutdown error");
        }
    }

    private void HandleEventDaemon()
    {
        while (!Closed || !_eventQueue.IsCompleted)
        {
            try
            {
                _handlingActions.Clear();
                _handlingActions.Add(_eventQueue.Take());
                while (_handlingActions.Count < EventQueueCapacity && _eventQueue.TryTake(out var action))
                {
                    _handlingActions.Add(action);
                }

                foreach (var action in _handlingActions)
                {
                    switch (action.Type)
                    {
                        case EventActionType.Event:
                            ProcessEvent(action.Event!);
                            break;
                        case EventActionType.Flush:
                            ProcessFlush();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                Loggers.Event?.Log(LogLevel.Error, e, "FeatureProbe event handle error");
            }
        }

        Loggers.Event?.Log(LogLevel.Information, "Event handler daemon shutdown");
    }

    private void ProcessEvent(BaseEvent @event)
    {
        _eventRepo.Add(@event);
    }

    private void ProcessFlush()
    {
        if (_eventRepo.Empty)
        {
            return;
        }

        var sendQueue = new List<EventRepository> { _eventRepo.Snapshot() };
        _eventRepo.Clear();

        _tasks.Add(_taskFactory.StartNew(() => SendEventsAsync(sendQueue).Wait()));
    }

    private async Task SendEventsAsync(List<EventRepository> sendQueue)
    {
        try
        {
            var resp = await _httpClient.PostAsync(_config.EventUrl, JsonContent.Create(sendQueue));
            if (!resp.IsSuccessStatusCode)
            {
                Loggers.Event?.Log(LogLevel.Error, "Http request error: {0}", resp.StatusCode);
                Loggers.Event?.Log(LogLevel.Debug, "Http response: {0}", JsonSerializer.Serialize(resp));
            }
        }
        catch (Exception e)
        {
            Loggers.Event?.Log(LogLevel.Error, e, "Unexpected error from event sender");
        }
    }

    private class EventRepository
    {
        public EventRepository()
        {
            Events = new List<BaseEvent>();
            Access = new AccessSummaryRecorder();
        }

        private EventRepository(EventRepository repo)
        {
            Events = repo.Events.ToList();
            Access = repo.Access.Snapshot();
        }

        [JsonPropertyName("events")] public List<BaseEvent> Events { get; }

        [JsonPropertyName("access")] public AccessSummaryRecorder Access { get; }

        [JsonIgnore] public bool Empty => Events.Count == 0 && Access.Counters.Count == 0;

        public void Add(BaseEvent @event)
        {
            if (@event is AccessEvent accessEvent)
            {
                Access.Add(accessEvent);
                if (accessEvent.TrackAccessEvents)
                {
                    Events.Add(accessEvent);
                }
            }
            else
            {
                Events.Add(@event);
            }
        }

        public EventRepository Snapshot()
        {
            return new EventRepository(this);
        }

        public void Clear()
        {
            Events.Clear();
            Access.Clear();
        }
    }

    private enum EventActionType
    {
        Event,
        Flush
    }

    private record EventAction(EventActionType Type, BaseEvent? Event);
}

public class DefaultEventProcessorFactory : IEventProcessorFactory
{
    public IEventProcessor Create(FPConfig config)
    {
        return new DefaultEventProcessor(config);
    }
}
