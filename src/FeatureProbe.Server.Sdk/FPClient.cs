using System.Text.Json;
using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Events;
using FeatureProbe.Server.Sdk.Internal;
using FeatureProbe.Server.Sdk.Models;
using FeatureProbe.Server.Sdk.Processors;
using FeatureProbe.Server.Sdk.Results;
using FeatureProbe.Server.Sdk.Synchronizer;
using Microsoft.Extensions.Logging;

namespace FeatureProbe.Server.Sdk;

/// <summary>
/// A client for the FeatureProbe API. Client instances are thread-safe.
/// Applications should instantiate a single FPClient for the lifetime of their application.
/// </summary>
public sealed class FPClient : IAsyncDisposable, IDisposable
{
    private readonly FPConfig _config;

    private readonly IEventProcessor _eventProcessor;

    private readonly IDataRepository _dataRepository;

    private readonly ISynchronizer _synchronizer;

    /// <summary>
    /// Gets FeatureProbe client initial state.
    /// </summary>
    public bool Initialized => this._dataRepository.Initialized;

    /// <summary>
    /// Creates a new client to connect to FeatureProbe with a custom configuration.
    /// </summary>
    /// <param name="config">the configuration control FeatureProbe client behavior</param>
    /// <param name="startWait">add a time limit for initializing the client (first fetch of data), in milliseconds, -1 to wait infinitely</param>
    public FPClient(FPConfig config, int startWait = 5000)
    {
        this._config = config;
        this._eventProcessor = config.EventProcessorFactory.Create(config);
        this._dataRepository = config.DataRepositoryFactory.Create(config);
        this._synchronizer = config.SynchronizerFactory.Create(config, this._dataRepository);

        try
        {
            if (!this._synchronizer.SynchronizeAsync().Wait(startWait))
            {
                Loggers.Main?.Log(LogLevel.Warning,
                    "Timeout encountered waiting for FeatureProbe client initialization (still running in background)");
            }
        }
        catch (Exception e)
        {
            Loggers.Main?.Log(LogLevel.Error, e, "Error encountered waiting for FeatureProbe client initialization");
        }

        if (!this._dataRepository.Initialized)
        {
            Loggers.Main?.Log(LogLevel.Warning, "FeatureProbe client was not successfully initialized");
        }
    }

    /// <summary>
    /// Creates a new client instance that connects to FeatureProbe with the default configuration.
    /// </summary>
    /// <param name="serverSdkKey">Server-side SDK key for your FeatureProbe environment</param>
    /// <param name="startWait">add a time limit for initializing the client (first fetch of data), in milliseconds, -1 to wait infinitely</param>
    public FPClient(string serverSdkKey, int startWait = 5000)
        : this(new FPConfig.Builder().ServerSdkKey(serverSdkKey).Build(), startWait)
    {
    }

    public bool BoolValue(string toggleKey, FPUser user, bool defaultValue)
        => GenericEvaluate(toggleKey, user, defaultValue);

    public string? StringValue(string toggleKey, FPUser user, string? defaultValue)
        => GenericEvaluate(toggleKey, user, defaultValue);

    public double NumberValue(string toggleKey, FPUser user, double defaultValue)
        => GenericEvaluate(toggleKey, user, defaultValue);

    public T? JsonValue<T>(string toggleKey, FPUser user, T? defaultValue)
        => GenericEvaluate(toggleKey, user, defaultValue, true);

    public FPDetail<bool> BoolDetail(string toggleKey, FPUser user, bool defaultValue)
        => GenericEvaluateDetail(toggleKey, user, defaultValue);

    public FPDetail<string> StringDetail(string toggleKey, FPUser user, string? defaultValue)
        => GenericEvaluateDetail(toggleKey, user, defaultValue);

    public FPDetail<double> NumberDetail(string toggleKey, FPUser user, double defaultValue)
        => GenericEvaluateDetail(toggleKey, user, defaultValue);

    public FPDetail<T> JsonDetail<T>(string toggleKey, FPUser user, T? defaultValue)
        => GenericEvaluateDetail(toggleKey, user, defaultValue, true);

    /// <summary>
    /// Tracks that a custom defined event, and optionally provides an additional numeric value for custom event.
    /// </summary>
    /// <param name="eventName">the name of the event</param>
    /// <param name="user">the user</param>
    /// <param name="value">optional numeric value</param>
    public void Track(string eventName, FPUser user, double? value = null)
    {
        _eventProcessor.Push(new CustomEvent(
            User: user.Key,
            Name: eventName,
            Value: value
        ));
    }

    /// <summary>
    /// Manually triggers an events push.
    /// </summary>
    public void Flush() => _eventProcessor.Flush();

    private T? GenericEvaluate<T>(string toggleKey, FPUser user, T? defaultValue, bool isJson = false)
    {
        var toggle = _dataRepository.GetToggle(toggleKey);
        if (toggle is null)
        {
            return defaultValue;
        }

        try
        {
            var evalResult = toggle.Eval(
                user,
                _dataRepository.Toggles,
                _dataRepository.Segments,
                defaultValue,
                _config.PrerequisiteDeep
            );
            TrackEvent(toggle, evalResult, user);
            // return isJson
            //     ? JsonSerializer.Deserialize<T?>(JsonSerializer.Serialize(evalResult.Value))
            //     : (T?)evalResult.Value;
            // FIXME: JsonElement cannot be cast to T

            return JsonSerializer.Deserialize<T?>(JsonSerializer.Serialize(evalResult.Value));
        }
        catch (Exception e)when (e is InvalidCastException or JsonException)
        {
            Loggers.Main?.Log(LogLevel.Error, e, "Toggle data type conversion error. toggleKey: {toggleKey}",
                toggleKey);
        }
        catch (Exception e)
        {
            Loggers.Main?.Log(LogLevel.Error, e, "FeatureProbe handle error. toggleKey: {toggleKey}", toggleKey);
        }

        return defaultValue;
    }

    private FPDetail<T> GenericEvaluateDetail<T>(string toggleKey, FPUser user, T? defaultValue, bool isJson = false)
    {
        if (!Initialized)
        {
            return new FPDetail<T>(
                Value: defaultValue,
                RuleIndex: null,
                Version: null,
                Reason: "FeatureProbe repository uninitialized"
            );
        }

        var toggle = _dataRepository.GetToggle(toggleKey);
        if (toggle is null)
        {
            return new FPDetail<T>(
                Value: defaultValue,
                RuleIndex: null,
                Version: null,
                Reason: "Toggle not exist"
            );
        }

        EvaluationResult? evalResult = null;
        T? evalValue = defaultValue;
        string? reason = null;
        try
        {
            evalResult = toggle.Eval(
                user,
                _dataRepository.Toggles,
                _dataRepository.Segments,
                defaultValue,
                _config.PrerequisiteDeep
            );
            // evalValue = isJson
            // ? JsonSerializer.Deserialize<T?>(JsonSerializer.Serialize(evalResult.Value))
            // : (T?)evalResult.Value;
            // FIXME: JsonElement cannot be cast to T

            evalValue = JsonSerializer.Deserialize<T?>(JsonSerializer.Serialize(evalResult.Value));
        }
        catch (Exception e) when (e is InvalidCastException or JsonException)
        {
            Loggers.Main?.Log(LogLevel.Error, e, "Toggle data type conversion error. toggleKey: {toggleKey}",
                toggleKey);
            reason = "Toggle data type mismatch";
        }
        catch (Exception e)
        {
            Loggers.Main?.Log(LogLevel.Error, e, "FeatureProbe handle error. toggleKey: {toggleKey}", toggleKey);
            reason = "FeatureProbe handle error";
        }

        var detail = new FPDetail<T>(
            Value: evalValue,
            RuleIndex: evalResult?.RuleIndex,
            Version: evalResult?.Version,
            Reason: reason ?? evalResult?.Reason
        );

        if (evalResult is not null) TrackEvent(toggle, evalResult, user);
        return detail;
    }

    private void TrackEvent(Toggle toggle, EvaluationResult evalResult, FPUser user)
    {
        _eventProcessor.Push(new AccessEvent(
            User: user.Key,
            Key: toggle.Key,
            Value: evalResult.Value,
            Version: evalResult.Version,
            RuleIndex: evalResult.RuleIndex,
            VariationIndex: evalResult.VariationIndex,
            TrackAccessEvents: toggle.TrackAccessEvents ?? false
        ));

        if (_dataRepository.DebugUntilTime >= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            _eventProcessor.Push(new DebugEvent(
                UserDetail: user,
                Key: toggle.Key,
                Value: evalResult.Value,
                Version: evalResult.Version,
                VariationIndex: evalResult.VariationIndex,
                RuleIndex: evalResult.RuleIndex,
                Reason: evalResult.Reason
            ));
        }
    }

    /// <summary>
    /// Safely shut down FeatureProbe instance.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _eventProcessor.ShutdownAsync();
        await _synchronizer.DisposeAsync();
        await _dataRepository.DisposeAsync();
    }

    /// <summary>
    /// Safely shut down FeatureProbe instance.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }
}
